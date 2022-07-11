using UnityEngine;

public class SlicingProgress : MonoBehaviour
{
    [SerializeField] private ComputeShader progressShader;
    
    private Texture3D targetVoxels;
    private int[] resultArray;
    private ComputeBuffer resultBuffer;
    
    private int threadGroupsCount;
    private int numPointsPerAxis;

    public void Init(int numPointsPerAxis, int threadGroupsCount, ComputeBuffer voxelsBuffer, RenderTexture correctionTexture, float correctionValue)
    {
        this.threadGroupsCount = threadGroupsCount;
        this.numPointsPerAxis = numPointsPerAxis;
        
        progressShader.SetInt("numPointsPerAxis",numPointsPerAxis);
        progressShader.SetFloat("correctionValue",correctionValue);
        progressShader.SetBuffer(0,"voxels",voxelsBuffer);
        progressShader.SetBuffer(1,"voxels",voxelsBuffer);
        progressShader.SetTexture(1,"correctionTexture",correctionTexture);

        resultBuffer = new ComputeBuffer(2, sizeof(int), ComputeBufferType.Raw);
        resultArray = new[] {0,0};
        progressShader.SetBuffer(0,"progress",resultBuffer);
    }
    
    public void UpdateTargetShape()
    {
        targetVoxels = LevelsConfig.GetSlicingShape();
        progressShader.SetTexture(0,"targetVoxels",targetVoxels);
        progressShader.SetTexture(1,"targetVoxels",targetVoxels);
    }

    public float UpdateShapeProgress()
    {
        resultArray[0] = 0;
        resultArray[1] = 0;
        resultBuffer.SetData(resultArray);
        progressShader.Dispatch(0,threadGroupsCount,threadGroupsCount,threadGroupsCount);

        resultBuffer.GetData(resultArray);
        return resultArray[1] / (float)resultArray[0];
    }

    public void ShapeCorrection()
    {
        progressShader.Dispatch(1,threadGroupsCount,threadGroupsCount,threadGroupsCount);
    }

    private void OnDestroy()
    {
        resultBuffer?.Release();
    }

    public float IsPointInSurface(Vector3 worldPoint,Vector3 velocity, float boundSize)
    {
        Vector3 voxelPoint = ConvertWorldToVoxelCoordinates(worldPoint, boundSize);
        Vector3Int id = new Vector3Int(Mathf.RoundToInt(voxelPoint.x), Mathf.RoundToInt(voxelPoint.y), Mathf.RoundToInt(voxelPoint.z));
        float voxelValue = targetVoxels.GetPixel(id.x, id.y, id.z).r;

        float dx = targetVoxels.GetPixel(id.x-1,id.y,id.z).r -  targetVoxels.GetPixel(id.x+1,id.y,id.z).r;
        float dy =  targetVoxels.GetPixel(id.x,id.y-1,id.z).r -  targetVoxels.GetPixel(id.x,id.y+1,id.z).r;
        float dz =  targetVoxels.GetPixel(id.x,id.y,id.z-1).r -targetVoxels.GetPixel(id.x,id.y,id.z+1).r;
        Vector3 normal = new Vector3(dx, dy, dz).normalized;


        if (voxelValue >= 0.2f)
            return voxelValue;

        if (voxelValue>=0f )
        {
            float dot = Vector3.Dot(-normal, velocity.normalized);
            if (dot <= 0.5f)
                dot = 0;
            return dot;
        }

        return 0;
    }
    
    private Vector3 ConvertWorldToVoxelCoordinates(Vector3 worldPoint, float boundsSize)
    {
        Vector3 minBoundsPoint = transform.position - Vector3.one*boundsSize / 2;

        Vector3 voxelPoint = ((worldPoint - minBoundsPoint) / boundsSize)*(numPointsPerAxis-1);
        voxelPoint.z = (numPointsPerAxis-1)/2f;
        return voxelPoint;
    }
}
