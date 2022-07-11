using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameTimer : MonoBehaviour
{
    [SerializeField] private float updateRate = 1;
    
    private float timePassedFromLastFPSUpdate = 0;
    private float fps;
    
    private void OnGUI()
    {
        if (timePassedFromLastFPSUpdate < Time.time)
        {
            timePassedFromLastFPSUpdate = Time.time + updateRate;
            fps = 1f / Time.deltaTime;
        }

        GUIStyle guiStyle = new GUIStyle {fontSize = 72};
        GUI.Label(new Rect(0, 0, 300, 300), fps.ToString("F"), guiStyle);
    }
}
