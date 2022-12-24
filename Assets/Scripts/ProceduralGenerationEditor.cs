#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralGeneration))]
public class ProceduralGenerationEditor : Editor
{
    /* 10 When the inspector renders it's GUI:
     * 
     * 20 Render the things that usually render
     */
    public override void OnInspectorGUI()// 10
    {
        var mapGenerator = (ProceduralGeneration)target;

        DrawDefaultInspector(); // 20

        if (GUILayout.Button("Generate"))
            mapGenerator.CreateMap();
    }
}
#endif