using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralGeneration))]
public class ProceduralGenerationEditor : Editor
{
    /* 10 When the inspector renders it's GUI:
     */
    public override void OnInspectorGUI()// 10
    {
        var mapGenerator = (ProceduralGeneration)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            mapGenerator.CreateMap();
    }
}
