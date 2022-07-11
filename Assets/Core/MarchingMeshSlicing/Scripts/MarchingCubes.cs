using System;
using MarchingCubesGPUProject;
using UnityEngine;
using UnityEngine.Rendering;

#pragma warning disable 162

public class MarchingCubes : MonoBehaviour
{
    const int threadGroupSize = 8;

    public static event Action OnSlicing;
    public static event Action OffSlicing;

    private const float SlicingSlow = 4;
    private const float ShapeSlicingSlow = 100;
    public static float SlowDownFactor { get; private set; }
 
    //todo: if need more voxel grid resolution than thread group size can be, need to split algorithm to chunks

    [Header("Voxel Settings")] 
    [SerializeField] private float boundsSize = 1;
    [Range(2, 100)] [SerializeField] private int numPointsPerAxis = 30; 
    [SerializeField] private Material drawMaterial;

    [Header("Density Generators")]
    [SerializeField] private DensityGenerator shape;
    [SerializeField] private DensityGenerator blade;

    [Header("ComputeShaders")]
    [SerializeField] private ComputeShader marchingCubes;
    [SerializeField] private ComputeShader normalsShader;

    [Header("Helper Components")] 
    [SerializeField] private ConnectedComponentsLabeling componentsLabeling;
    [SerializeField] private SlicingProgress slicingProgress;
    [SerializeField] private Painting painting;
    
    [Header("Gizmos")] 
    [SerializeField] private bool showBoundsGizmo = true;
    [SerializeField] private Color boundsGizmoCol = Color.white;

    ComputeBuffer voxelsBuffer, vertexBuffer, surfacePoints, helperCountBuffer;
    ComputeBuffer cubeEdgeFlags, triangleConnectionTable;

    RenderTexture normalsBuffer;

    private int numThreadsPerAxis;
    private int[] helperCountArray = {0,0};
    private int vertexCount;
    private float pointSpacing;
    
    private float clearDiscardedVoxelsTimer;

    private bool _isSlicing;
    private bool IsSlicing
    {
        set
        {
            if (!_isSlicing && value) OnSlicing?.Invoke();
            else if (_isSlicing && !value) OffSlicing?.Invoke();
            _isSlicing = value;
        }
    }

    private void OnEnable()
    {
        ChainSaw.OnMove += TryToUpdateGeometry;
        ChainSaw.OnMoveEnd += ResetSlicingState;
        StickerRoll.OnStickerEnabled += OnStickerEnabled;
        Dryer.OnDryer += ShapeCorrection;
        GameManager.OnStopInstrument += ResetDiscarded;
    }

    private void OnDisable()
    {
        ChainSaw.OnMove -= TryToUpdateGeometry;
        ChainSaw.OnMoveEnd -= ResetSlicingState;
        StickerRoll.OnStickerEnabled -= OnStickerEnabled;
        Dryer.OnDryer -= ShapeCorrection;
        GameManager.OnStopInstrument -= ResetDiscarded;
    }

    void Awake()
    { 
        numThreadsPerAxis = Mathf.CeilToInt (numPointsPerAxis / (float) threadGroupSize);
        pointSpacing = boundsSize / (numPointsPerAxis - 1);

        CreateBuffers();
        LinkBuffers();
        
        painting.Init(voxelsBuffer,boundsSize,numPointsPerAxis,numThreadsPerAxis,drawMaterial);
        componentsLabeling.Init(numPointsPerAxis,numThreadsPerAxis,voxelsBuffer);
        shape.Init(voxelsBuffer, numPointsPerAxis, boundsSize, Vector3.one * boundsSize, pointSpacing);
        blade.Init(voxelsBuffer, numPointsPerAxis, boundsSize, Vector3.one * boundsSize, pointSpacing,helperCountBuffer);
        slicingProgress.Init(numPointsPerAxis,numThreadsPerAxis,voxelsBuffer,painting.GetTexture("_OpacityTexture"),painting.GetLayerMaxValue("_OpacityTexture"));
    }

    public void RespawnShape()
    {
        componentsLabeling.UpdateTargetShape();
        slicingProgress.UpdateTargetShape();
        shape.Generate();
        UpdateGeometry();
        painting.Clear();
        
        drawMaterial.SetFloat("_EnablePaint",0); //disable painting shader part when starting stage
    }
    
    private void ResetDiscarded()
    {
        //clear discarded
        if (!LevelsConfig.IsSpray)
        {
            helperCountArray[0] = 0;
            helperCountArray[1] = 0;
            helperCountBuffer.SetData(helperCountArray);
    
            //find surface points
            marchingCubes.Dispatch(2,numThreadsPerAxis,numThreadsPerAxis,numThreadsPerAxis);
        
            //clear discarded voxels & set their value =  - min distance to surface
            marchingCubes.Dispatch(1,numThreadsPerAxis,numThreadsPerAxis,numThreadsPerAxis);
            UpdateGeometry();
        }
    }

    public float GetProgress()
    {
        float shapeProgress = slicingProgress.UpdateShapeProgress();
        float opacityProgress = painting.GetProgress("_OpacityTexture");
        float sprayProgress = painting.GetProgressWithMask("_SprayTexture", (Texture2D)Shader.GetGlobalTexture("_UnlockableTexture"));
        return (shapeProgress+opacityProgress+sprayProgress)/3f;
    }

    private float[] OnStickerEnabled()
    {
        Shader.SetGlobalTexture("_SprayTexture",painting.GetTexture("_SprayTexture"));
        (RenderTexture shapeTexture, float[] uvBounds) = painting.GetShapeData("_ShapeTexture");
        Shader.SetGlobalTexture("_ShapeTexture" ,shapeTexture);
        
        drawMaterial.SetFloat("_EnablePaint",1); //enable painting shader part
        return uvBounds;
    }

    private void CreateBuffers()
    {
        int voxelGridSize = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;

        //Holds the voxel values, generated from density functions
        voxelsBuffer = new ComputeBuffer(voxelGridSize, sizeof(float)*2);

        //Holds the verts generated by the marching cubes
        int maxVertCount = (numPointsPerAxis-1) * (numPointsPerAxis-1) * (numPointsPerAxis-1) * 3 * 5; //can be max 5 triangles per voxel
        vertexBuffer = new ComputeBuffer(maxVertCount, sizeof(float) * 11,ComputeBufferType.Append); //7 = 4 floats for position + 3 floats for normal
        helperCountBuffer = new ComputeBuffer(2, sizeof(int), ComputeBufferType.Raw);
        surfacePoints = new ComputeBuffer(voxelGridSize, sizeof(float) * 3, ComputeBufferType.Append);
        
        //Holds the normals of the voxels.
        normalsBuffer = new RenderTexture(numPointsPerAxis, numPointsPerAxis, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear);
        normalsBuffer.dimension = TextureDimension.Tex3D;
        normalsBuffer.enableRandomWrite = true;
        normalsBuffer.useMipMap = false;
        normalsBuffer.volumeDepth = numPointsPerAxis; //3d texture
        normalsBuffer.Create();
        
        //These two buffers are just some settings needed by the marching cubes.
        cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
        cubeEdgeFlags.SetData(MarchingCubesTables.CubeEdgeFlags);
        triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
        triangleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);
    }

    private void LinkBuffers()
    {
        normalsShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        normalsShader.SetBuffer(0, "voxels", voxelsBuffer);
        normalsShader.SetTexture(0, "normals", normalsBuffer);
        
        marchingCubes.SetInt("numPointsPerAxis", numPointsPerAxis);
        marchingCubes.SetBuffer(0, "_Voxels", voxelsBuffer);
        marchingCubes.SetTexture(0, "_Normals", normalsBuffer);
        marchingCubes.SetBuffer(0, "_Vertices", vertexBuffer);
        marchingCubes.SetVector("centre", transform.position);
        marchingCubes.SetFloat("boundsSize", boundsSize);
        marchingCubes.SetBuffer(0, "CubeEdgeFlags", cubeEdgeFlags);
        marchingCubes.SetBuffer(0, "TriangleConnectionTable", triangleConnectionTable);
        marchingCubes.SetBuffer(1, "_Voxels", voxelsBuffer);
        marchingCubes.SetBuffer(1,"helperArgs",helperCountBuffer);
        marchingCubes.SetBuffer(1, "surfacePointsRead", surfacePoints);
        marchingCubes.SetBuffer(2,"helperArgs",helperCountBuffer);
        marchingCubes.SetBuffer(2, "_Voxels", voxelsBuffer);
        marchingCubes.SetBuffer(2, "surfacePoints", surfacePoints);
    }

    private void TryToUpdateGeometry(Vector3 bladePosition, Vector3 velocity)
    {
        if ((bladePosition - transform.position).magnitude < (boundsSize / 2f))
        {
            UpdateGeometryOnSlice();

            if (_isSlicing)
            {
                if (SlowDownFactor < SlicingSlow) //minimum slow down
                    SlowDownFactor = SlicingSlow;

                float targetSlow = Mathf.Lerp(SlicingSlow, ShapeSlicingSlow, slicingProgress.IsPointInSurface(bladePosition,velocity, boundsSize));
                SlowDownFactor = Mathf.Lerp(SlowDownFactor,targetSlow,3*Time.deltaTime);
            }
        }
        else
            IsSlicing = false;
    }

    private void Update()
    {
        if (_isSlicing)
            clearDiscardedVoxelsTimer = 0;
        else
        {
            clearDiscardedVoxelsTimer += Time.deltaTime;
            SlowDownFactor = Mathf.Lerp(SlowDownFactor, 1, 5 * Time.deltaTime);
        }
    }

    //ensure sync with vertexCount
    private void LateUpdate()
    {
        Graphics.DrawProcedural(drawMaterial,new Bounds(transform.position,Vector3.one*boundsSize),MeshTopology.Triangles,vertexCount);
    }

    private void ResetSlicingState() => IsSlicing = false;

    private void UpdateGeometry()
    {
        helperCountArray[0] = 0;
        helperCountArray[1] = 0;
        helperCountBuffer.SetData(helperCountArray); //performance cost?
        vertexBuffer.SetCounterValue(0); //clear the vertex buffer from the last frame

        //Make the voxel normals.
        normalsShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
        
        //Make the mesh verts
        marchingCubes.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
        
        ComputeBuffer.CopyCount(vertexBuffer,helperCountBuffer,4);
        helperCountBuffer.GetData(helperCountArray);
        vertexCount = helperCountArray[1]*3; //vertex buffer appending 3 vertices as one struct for the right vertices order, so counter is multiplied by 3
        drawMaterial.SetBuffer("_Buffer", vertexBuffer);
    }

    private void UpdateGeometryOnSlice()
    {
        if (clearDiscardedVoxelsTimer > 3)
        {
            clearDiscardedVoxelsTimer = 0;
            
            helperCountArray[0] = 0;
            helperCountArray[1] = 0;
            helperCountBuffer.SetData(helperCountArray);
        
            //find surface points
            marchingCubes.Dispatch(2,numThreadsPerAxis,numThreadsPerAxis,numThreadsPerAxis);
            
            //clear discarded voxels & set their value =  - min distance to surface
            marchingCubes.Dispatch(1,numThreadsPerAxis,numThreadsPerAxis,numThreadsPerAxis);
        }
        
        helperCountArray[0] = 0;
        helperCountArray[1] = 0;
        helperCountBuffer.SetData(helperCountArray); //performance cost?
        vertexBuffer.SetCounterValue(0); //clear the vertex buffer from the last frame

        //todo: separate slice & density logic? (needs additional voxels Buffer)
        //Generate blade shape and cut it fom cube
        blade.Generate();
        
        //Discard disconnected  voxels
        componentsLabeling.DiscardDisconnectedAreas();
        
        //Make the voxel normals.
        normalsShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
        
        //Make the mesh verts
        marchingCubes.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
        
        ComputeBuffer.CopyCount(vertexBuffer,helperCountBuffer,4);
        helperCountBuffer.GetData(helperCountArray);
        vertexCount = helperCountArray[1]*3; //vertex buffer appending 3 vertices as one struct for the right vertices order, so counter is multiplied by 3
        IsSlicing = helperCountArray[0] > 0;
        drawMaterial.SetBuffer("_Buffer", vertexBuffer);
    }
    
    private void ShapeCorrection()
    {
        slicingProgress.ShapeCorrection();
        
        //Discard disconnected  voxels (needed for grown in air ice part to be discarded, temp solution non-optimized)
        componentsLabeling.DiscardDisconnectedAreas(true);
        UpdateGeometry();
    }

    void OnDestroy()
    {
        //MUST release buffers.
        voxelsBuffer?.Release();
        vertexBuffer?.Release();
        helperCountBuffer?.Release();
        normalsBuffer?.Release();
        surfacePoints?.Release();
        
        cubeEdgeFlags?.Release();
        triangleConnectionTable?.Release();
    }

    void OnDrawGizmos()
    { 
        if (showBoundsGizmo)
        {
            Gizmos.color = boundsGizmoCol;
            Gizmos.color = boundsGizmoCol;
            Gizmos.DrawWireCube(transform.position, Vector3.one * boundsSize);
        }
    }
}