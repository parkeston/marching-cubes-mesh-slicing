using System.Text;
using UnityEngine;

public class ConnectedComponentsLabeling : MonoBehaviour
{
    [SerializeField] private ComputeShader voxelsDataNormalizeCompute;
    [SerializeField] private ComputeShader componentsLabelingCompute;
    [SerializeField] private ComputeShader discardVoxelsCompute;

    private ComputeBuffer voxelsBuffer;
    private ComputeBuffer voxelsNormalizedBuffer;
    private ComputeBuffer labelsBuffer;

    private ComputeBuffer labelGroupsSizes;
    private ComputeBuffer helperArgs;

    private int threadGroupsCount;
    
    public void Init(int numPointsPerAxis, int threadGroupsCount, ComputeBuffer voxelsBuffer)
    {
        this.threadGroupsCount = threadGroupsCount;
        this.voxelsBuffer = voxelsBuffer;
        CreateBuffers(numPointsPerAxis*numPointsPerAxis*numPointsPerAxis);
        LinkBuffers(numPointsPerAxis);
    }
    
    private void CreateBuffers(int size)
    {
        voxelsNormalizedBuffer = new ComputeBuffer(size, sizeof(int));
        labelsBuffer = new ComputeBuffer(size, sizeof(int));

        labelGroupsSizes = new ComputeBuffer(size, sizeof(int));
        helperArgs = new ComputeBuffer(3, sizeof(int));
    }
    
    private void LinkBuffers(int numPointsPerAxis)
    {
        voxelsDataNormalizeCompute.SetInt("numPointsPerAxis",numPointsPerAxis);
        voxelsDataNormalizeCompute.SetBuffer(0,"voxels",voxelsBuffer);
        voxelsDataNormalizeCompute.SetBuffer(0,"voxelsNormalized",voxelsNormalizedBuffer);
        
        componentsLabelingCompute.SetInt("numPointsPerAxis",numPointsPerAxis);
        componentsLabelingCompute.SetBuffer(0,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(1,"voxels",voxelsNormalizedBuffer);
        componentsLabelingCompute.SetBuffer(1,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(2,"voxels",voxelsNormalizedBuffer);
        componentsLabelingCompute.SetBuffer(2,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(3,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(4,"voxels",voxelsNormalizedBuffer);
        componentsLabelingCompute.SetBuffer(4,"labels",labelsBuffer);
        componentsLabelingCompute.SetBuffer(5,"voxels",voxelsNormalizedBuffer);
        componentsLabelingCompute.SetBuffer(5,"labels",labelsBuffer);
        
        discardVoxelsCompute.SetInt("numPointsPerAxis",numPointsPerAxis);
        discardVoxelsCompute.SetFloat("time",Time.timeSinceLevelLoad);
        discardVoxelsCompute.SetBuffer(0,"labelGroupsSizes",labelGroupsSizes);
        discardVoxelsCompute.SetBuffer(0,"helperArgs",helperArgs);
        discardVoxelsCompute.SetBuffer(1,"voxels",voxelsBuffer);
        discardVoxelsCompute.SetBuffer(1,"labels",labelsBuffer);
        discardVoxelsCompute.SetBuffer(1,"labelGroupsSizes",labelGroupsSizes);
        discardVoxelsCompute.SetBuffer(2,"labelGroupsSizes",labelGroupsSizes);
        discardVoxelsCompute.SetBuffer(2,"helperArgs",helperArgs);
        discardVoxelsCompute.SetBuffer(3,"labelGroupsSizes",labelGroupsSizes);
        discardVoxelsCompute.SetBuffer(3,"helperArgs",helperArgs);
        discardVoxelsCompute.SetBuffer(4,"voxels",voxelsBuffer);
        discardVoxelsCompute.SetBuffer(4,"helperArgs",helperArgs);
        discardVoxelsCompute.SetBuffer(4,"labels",labelsBuffer);
        discardVoxelsCompute.SetBuffer(5,"helperArgs",helperArgs);
        discardVoxelsCompute.SetBuffer(5,"labelGroupsSizes",labelGroupsSizes);
    }
    
    public void UpdateTargetShape()
    {
        var targetVoxels = LevelsConfig.GetSlicingShape();
        discardVoxelsCompute.SetTexture(1,"targetVoxels",targetVoxels);
    }
    
    public void DiscardDisconnectedAreas(bool immediateDiscard=false)
    {
        //normalize voxel data (1 - has surface ,0 - no surface)
        voxelsDataNormalizeCompute.Dispatch(0,threadGroupsCount,threadGroupsCount,threadGroupsCount);

        //init labels list
        componentsLabelingCompute.Dispatch(0, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        
        //voxels grid row scanning
        componentsLabelingCompute.Dispatch(1, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        
        //voxels grid column scanning
        componentsLabelingCompute.Dispatch(2, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        
        //voxels grid depth scanning
        componentsLabelingCompute.Dispatch(5, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        
        //finding labels roots
        componentsLabelingCompute.Dispatch(3, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        
        //refining labels values
        componentsLabelingCompute.Dispatch(4, threadGroupsCount, threadGroupsCount, threadGroupsCount);
        
        //finding labels roots once again
        componentsLabelingCompute.Dispatch(3, threadGroupsCount, threadGroupsCount, threadGroupsCount);

        discardVoxelsCompute.SetFloat("time",immediateDiscard?0.1f:Time.timeSinceLevelLoad);
        //clear previous label groups
        discardVoxelsCompute.Dispatch(0,threadGroupsCount,threadGroupsCount,threadGroupsCount);
        
        //count same labels
        discardVoxelsCompute.Dispatch(1,threadGroupsCount,threadGroupsCount,threadGroupsCount);
        
        //count label groups
        discardVoxelsCompute.Dispatch(2,threadGroupsCount,threadGroupsCount,threadGroupsCount);
        
        //find min size of label groups
        discardVoxelsCompute.Dispatch(3,threadGroupsCount,threadGroupsCount,threadGroupsCount);
        
        //find label of min size label group
        discardVoxelsCompute.Dispatch(5,threadGroupsCount,threadGroupsCount,threadGroupsCount);
        
        //discard min size label group
        discardVoxelsCompute.Dispatch(4,threadGroupsCount,threadGroupsCount,threadGroupsCount);
    }
    
    void OnDestroy()
    {
        //MUST release buffers.
        voxelsNormalizedBuffer?.Release();
        labelsBuffer?.Release();
        labelGroupsSizes?.Release();
        helperArgs?.Release();
    }
}
