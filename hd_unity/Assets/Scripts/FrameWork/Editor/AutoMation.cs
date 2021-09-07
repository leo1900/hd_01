using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AutoMation : Editor
{
    [MenuItem("Automation/Tools/Build/AndroidBuild", false, 5)]
    public static void AndroidBuild()
    {
        // Build(BuildTarget.Android);
        Debug.Log("AndroidBuild  Start-----------------------");
    }
     
    [MenuItem("Automation/Tools/Build/iOSBuild", false, 4)]
    public static void iOSBuild()
    {
        Debug.Log("iOSBuild  Start-----------------------");
    } 
}
