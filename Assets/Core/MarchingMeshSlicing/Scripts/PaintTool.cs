using System;
using UnityEngine;

// use or inherit from this class to delegate painting features
public class PaintTool: MonoBehaviour
{
    public static event Action<string,Ray,float,float> OnPainted;

    [Header("Paint Tool Settings")] 
    [SerializeField] private string layerName;
    [SerializeField] private float radius;
    [SerializeField] private float strength;
    [SerializeField] private float speed;

    [Space] 
    [SerializeField] private Transform paintTransform;
    [SerializeField] private Vector3 localPaintDirection;
    
    protected virtual void InvokePaintEvent()
    {
        OnPainted?.Invoke(layerName,new Ray(paintTransform.position,paintTransform.TransformDirection(localPaintDirection)), 
            radius,speed*Time.deltaTime*strength);
    }
    
    protected virtual void Update()
    {
        if(Input.GetMouseButton(0))
            InvokePaintEvent();
    }
}
