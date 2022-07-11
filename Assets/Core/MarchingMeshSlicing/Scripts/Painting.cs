using System;
using UnityEngine;


public class Painting : MonoBehaviour
{
    [SerializeField] private ComputeShader paintingShader;
    [SerializeField] private float paintingPlaneZ;
    [SerializeField] private PaintLayer[] paintLayers;

    private Plane paintingPlane;
    private int numThreadsPerAxis;
    private int numPointsPerAxis;
    private float boundsSize;

    private ComputeBuffer progressBuffer;
    private int[] progressArray;
    
    private ComputeBuffer uvBoundsBuffer;
    private int[] uvBoundsArray;
    
    private void OnEnable()
    {
        PaintTool.OnPainted += Paint;
    }

    private void OnDisable()
    {
        PaintTool.OnPainted -= Paint;
    }

    public void Init(ComputeBuffer voxelsBuffer, float boundsSize, int numPointsPerAxis,int numThreadsPerAxis, Material drawMaterial)
    {
        this.boundsSize = boundsSize;
        this.numPointsPerAxis = numPointsPerAxis;
        this.numThreadsPerAxis = numThreadsPerAxis;
        paintingPlane = new Plane(Vector3.back, transform.position + Vector3.back * paintingPlaneZ);

        foreach (var paintLayer in paintLayers)
        {
            paintLayer.CreateTexture(numPointsPerAxis); //todo: try to reuse & bake textures
            drawMaterial.SetTexture(paintLayer.Name,paintLayer.Texture);
        }

        paintingShader.SetInt("numPointsPerAxis",numPointsPerAxis);
        paintingShader.SetBuffer(2,"voxels",voxelsBuffer);
        paintingShader.SetBuffer(3,"voxels",voxelsBuffer);
        paintingShader.SetBuffer(4,"voxels",voxelsBuffer);

        progressBuffer = new ComputeBuffer(2, sizeof(int), ComputeBufferType.Raw);
        progressArray = new[] {0,0};
        paintingShader.SetBuffer(2,"progress",progressBuffer);
        paintingShader.SetBuffer(4,"progress",progressBuffer);

        uvBoundsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.Raw);
        uvBoundsArray = new[] {0, 0, 0, 0};
        paintingShader.SetBuffer(3,"uvBounds",uvBoundsBuffer);
        paintingShader.SetBuffer(4,"uvBounds",uvBoundsBuffer);
    }

    public void Clear()
    {
        foreach (var paintLayer in paintLayers)
        {
            paintingShader.SetTexture(1, "paintTextureWrite", paintLayer.Texture);
            paintingShader.Dispatch(1,numThreadsPerAxis,numThreadsPerAxis,numThreadsPerAxis);
        }
    }
    
    public float GetProgress(string layerName)
    {
        var currentLayer = GetLayer(layerName);
        
        progressArray[0] = 0;
        progressArray[1] = 0;
        progressBuffer.SetData(progressArray);
        
        paintingShader.SetTexture(2,"paintTextureRead",currentLayer.Texture);
        paintingShader.SetFloat("paintMaxValue",currentLayer.MaxLayerValue);
        paintingShader.Dispatch(2,numThreadsPerAxis,numThreadsPerAxis,numThreadsPerAxis);

        progressBuffer.GetData(progressArray);
        return (float)progressArray[1] / progressArray[0];
    }

    public float GetProgressWithMask (string layerName, Texture2D mask)
    {
        var currentLayer = GetLayer(layerName);
        
        progressArray[0] = 0;
        progressArray[1] = 0;
        progressBuffer.SetData(progressArray);
        
        paintingShader.SetTexture(4,"paintTextureRead",currentLayer.Texture);
        paintingShader.SetTexture(4,"maskTexture",mask);
        paintingShader.SetFloat("paintMaxValue",currentLayer.MaxLayerValue);
        paintingShader.Dispatch(4,numThreadsPerAxis,numThreadsPerAxis,numThreadsPerAxis);

        progressBuffer.GetData(progressArray);
        return (float)progressArray[1] / progressArray[0];
    }
    
    public (RenderTexture,float[]) GetShapeData(string layerName)
    {
        uvBoundsArray[0] = numPointsPerAxis - 1; //for min y voxel search
        uvBoundsArray[1] = 0; //for max y voxel search
        uvBoundsArray[2] = numPointsPerAxis - 1; //for min x voxel search
        uvBoundsArray[3] = 0; //for max x voxel search
        uvBoundsBuffer.SetData(uvBoundsArray);
        
        var currentLayer = GetLayer(layerName);
        paintingShader.SetTexture(3,"paintTextureWrite",currentLayer.Texture);
        paintingShader.Dispatch(3,numThreadsPerAxis,numThreadsPerAxis,1);
        
        uvBoundsBuffer.GetData(uvBoundsArray);
        float minVPoint = uvBoundsArray[0] / (numPointsPerAxis - 1f);
        float maxVPoint = uvBoundsArray[1] / (numPointsPerAxis - 1f);
        float minUPoint = uvBoundsArray[2] / (numPointsPerAxis - 1f);
        float maxUPoint = uvBoundsArray[3] / (numPointsPerAxis - 1f);
        
        return (currentLayer.Texture,new[]{minVPoint,maxVPoint,minUPoint,maxUPoint});
    }

    public RenderTexture GetTexture(string layerName)
    {
        var currentLayer = GetLayer(layerName);
        return currentLayer.Texture;
    }

    public float GetLayerMaxValue(string layerName) => GetLayer(layerName).MaxLayerValue;

    private void Paint(string layerName, Ray paintRay, float radius, float delta)
    {
        if (paintingPlane.Raycast(paintRay, out float distance))
        {
            Vector3 point = paintRay.GetPoint(distance);
            point = ConvertWorldToVoxelCoordinates(point);

            var currentLayer = GetLayer(layerName);
            paintingShader.SetTexture(0, "paintTextureWrite", currentLayer.Texture);
            paintingShader.SetTexture(0, "paintTextureRead", currentLayer.Texture);
            paintingShader.SetFloat("paintMaxValue",currentLayer.MaxLayerValue);
            
            paintingShader.SetVector("paintPos",point);
            paintingShader.SetFloat("paintRadius",radius);
            paintingShader.SetFloat("paintDelta",delta);

            paintingShader.Dispatch(0,numThreadsPerAxis,numThreadsPerAxis,numThreadsPerAxis);
        }
    }

    private Vector3 ConvertWorldToVoxelCoordinates(Vector3 worldPoint)
    {
        Vector3 minBoundsPoint = transform.position - Vector3.one*boundsSize / 2;

        Vector3 voxelPoint = ((worldPoint - minBoundsPoint) / boundsSize)*(numPointsPerAxis-1);
        voxelPoint.z = 0; //ignore depth in paiting
        return voxelPoint;
    }

    private PaintLayer GetLayer(string name)
    {
        foreach (var paintLayer in paintLayers)
        {
            if (paintLayer.Name == name)
                return paintLayer;
        }

        return null;
    }

    private void OnDestroy()
    {
        progressBuffer?.Release();
    }

    [Serializable]
    private class PaintLayer
    {
        [SerializeField] private string name;
        [SerializeField] private float maxLayerValue;
        [SerializeField] private RenderTexture texture;

        public string Name => name;
        public float MaxLayerValue => maxLayerValue;
        public RenderTexture Texture => texture;
        
        public void CreateTexture(int size)
        {
            texture = new RenderTexture(size, size, 0,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear);
            texture.enableRandomWrite = true;
            texture.useMipMap = false;
            texture.Create();
        }
    }
}
