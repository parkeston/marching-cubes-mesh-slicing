using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BootLoader
{
    [RuntimeInitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        Application.targetFrameRate = 60;
    }
}
