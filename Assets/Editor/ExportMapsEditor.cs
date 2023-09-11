using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExportMaps))]
public class ExportMapsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ExportMaps script = (ExportMaps)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Esporta Mappe"))
        {
            script.ExportAllMaps();
        }
    }
}
