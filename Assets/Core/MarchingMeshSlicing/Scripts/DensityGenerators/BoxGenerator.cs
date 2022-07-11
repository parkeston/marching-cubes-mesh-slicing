using UnityEngine;

public class BoxGenerator : DensityGenerator
{
   [SerializeField] private Vector3 boxDimensions;
   
   public override void Init(ComputeBuffer voxelsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, float spacing,ComputeBuffer argsBuffer = null)
   {
      densityShader.SetVector("boxDimensions",boxDimensions);
      base.Init(voxelsBuffer, numPointsPerAxis, boundsSize, worldBounds, spacing,argsBuffer);
   }
}
