using System;
using UnityEngine;

public class BladeGenerator : DensityGenerator
{
    [SerializeField] private Vector3 bladeDimensions;

    private Vector3 originPosition;
    private float originRotation;
    
    private void Awake()
    {
        originPosition = Vector3.zero;
        originRotation = 0;
    }

    public override void Init(ComputeBuffer voxelsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, float spacing,ComputeBuffer argsBuffer = null)
    {
        densityShader.SetVector("bladeDimensions",bladeDimensions);
        base.Init(voxelsBuffer, numPointsPerAxis, boundsSize, worldBounds, spacing,argsBuffer);
    }

    protected override void OnGenerate()
    {
        densityShader.SetVector("translationOffset",transform.position-originPosition);
        densityShader.SetFloat("rotation",(transform.eulerAngles.z-originRotation));
    }
}
