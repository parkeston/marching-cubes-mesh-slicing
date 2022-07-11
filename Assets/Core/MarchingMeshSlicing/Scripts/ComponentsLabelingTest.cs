using UnityEngine;

//WIP
public class ComponentsLabelingTest : MonoBehaviour
{
    private const int threadGroupSize = 8;

    [SerializeField] private Cell cellPrefab;
    [SerializeField] private float cellSize;
    
    [Space]
    [SerializeField] private ComputeShader componentsLabelingCompute;
    [SerializeField] private int numPointsPerAxis;

    [Header("Debug")]
    [Range(1,7)][SerializeField] private int steps;

    //0 - no surface, 1 - has surface
    private int[] voxels;
    private int[] labels;
    private Cell[] cells;

    private ComputeBuffer voxelsBuffer;
    private ComputeBuffer labelsBuffer;

    private int threadGroupsCount;

    private Vector3 IndexTo3DIndex(int i) => new Vector3(i % numPointsPerAxis, 
        ( i / numPointsPerAxis) % numPointsPerAxis, i / (numPointsPerAxis * numPointsPerAxis));

    private void Start()
    {
        int voxelGridSize = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        
        voxels = new int[voxelGridSize];
        labels = new int[voxelGridSize];
        
        cells = new Cell[voxelGridSize];
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = Instantiate(cellPrefab, IndexTo3DIndex(i) * cellSize*2f, Quaternion.identity);
            cells[i].transform.localScale = Vector3.one * cellSize;

            int k = i;
            cells[i].SetOnClickAction(()=>
            {
                voxels[k] = voxels[k] == 0 ? 1 : 0;
                UpdateLabels();
            });
        }
        
        voxelsBuffer = new ComputeBuffer(voxelGridSize, sizeof(int));
        labelsBuffer = new ComputeBuffer(voxelGridSize, sizeof(int));
        
        //set appropriate buffers for each kernel
        componentsLabelingCompute.SetInt("numPointsPerAxis",numPointsPerAxis);
        componentsLabelingCompute.SetBuffer(0,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(1,"voxels",voxelsBuffer);
        componentsLabelingCompute.SetBuffer(1,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(2,"voxels",voxelsBuffer);
        componentsLabelingCompute.SetBuffer(2,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(3,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(4,"voxels",voxelsBuffer);
        componentsLabelingCompute.SetBuffer(4,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(5,"voxels",voxelsBuffer);
        componentsLabelingCompute.SetBuffer(5,"labels",labelsBuffer);

        threadGroupsCount = Mathf.CeilToInt((float) numPointsPerAxis/ threadGroupSize);
        
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        voxelsBuffer.SetData(voxels); //perfomance crticial, for testing ok
        
        if (steps >= 1)
        {
            int kernel = componentsLabelingCompute.FindKernel("init");
            componentsLabelingCompute.Dispatch(kernel, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        }

        if (steps >= 2)
        {
            int kernel = componentsLabelingCompute.FindKernel("rowScan");
            componentsLabelingCompute.Dispatch(kernel, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        }

        if (steps >= 3)
        {
            int kernel = componentsLabelingCompute.FindKernel("colScan");
            componentsLabelingCompute.Dispatch(kernel, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        }

        if (steps >= 4)
        {
            int kernel = componentsLabelingCompute.FindKernel("depthScan");
            componentsLabelingCompute.Dispatch(kernel, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        }

        if (steps >= 5)
        {
            int kernel = componentsLabelingCompute.FindKernel("findRoots");
            componentsLabelingCompute.Dispatch(kernel, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        }

        if (steps >= 6)
        {
            int kernel = componentsLabelingCompute.FindKernel("refine");
            //todo: single threaded,cause of atomic problem, see compute shader for details
            componentsLabelingCompute.Dispatch(kernel, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        }

        if (steps >= 7)
        {
            int kernel = componentsLabelingCompute.FindKernel("findRoots");
            componentsLabelingCompute.Dispatch(kernel, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        }

        labelsBuffer.GetData(labels); //performance critical, but for testing visualization ok
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].UpdateAppearance(voxels[i],labels[i]);
        }
    }

    private void OnValidate()
    {
        if(Application.isPlaying && cells!=null)
            UpdateLabels();
    }
}
