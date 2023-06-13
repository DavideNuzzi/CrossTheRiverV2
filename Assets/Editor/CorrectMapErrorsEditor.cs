using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CorrectMapErrors))]
public class CorrectMapErrorsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CorrectMapErrors correctionScript = (CorrectMapErrors)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Imposta dimensioni piattaforme"))
        {
            correctionScript.ChangePlatformSizes();
        }

        if (GUILayout.Button("Imposta tempo livelli"))
        {
            correctionScript.ChangeLevelTime();

        }
    }
}
