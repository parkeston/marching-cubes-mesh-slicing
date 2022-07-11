using System.Collections.Generic;
using UnityEngine;

public class DensityGenerator : MonoBehaviour
{
    const int threadGroupSize = 8;
    
    [SerializeField] protected ComputeShader densityShader;
    [SerializeField] private string argsBufferName = "argsBuffer";
    [SerializeField] private Vector3 relativeCentre;

    protected int numThreadsPerAxis;
    protected List<ComputeBuffer> buffersToRelease;

    public virtual void Init (ComputeBuffer voxelsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, float spacing, ComputeBuffer argsBuffer = null) 
    {
        numThreadsPerAxis = Mathf.CeilToInt (numPointsPerAxis / (float) threadGroupSize);
        
        densityShader.SetBuffer (0, "voxels", voxelsBuffer);
        densityShader.SetInt ("numPointsPerAxis", numPointsPerAxis);
        densityShader.SetFloat ("boundsSize", boundsSize);
        densityShader.SetVector ("centre", new Vector4 (relativeCentre.x, relativeCentre.y, relativeCentre.z));
        densityShader.SetFloat ("spacing", spacing);
        densityShader.SetVector("worldSize", worldBounds);
        
        if(argsBuffer!=null)
            densityShader.SetBuffer(0,argsBufferName,argsBuffer);
    }

    protected virtual void OnGenerate(){ }

    public void Generate()
    {
        OnGenerate();
        densityShader.Dispatch (0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        if (buffersToRelease != null) {
            foreach (var b in buffersToRelease) {
                b.Release();
            }
        }
    }
}
