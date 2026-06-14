#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainMeshBuilder))]
public class TerrainMeshBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Build Terrain"))
        {
            var builder = (TerrainMeshBuilder)target;
            builder.BuildTerrain();
        }
    }
}
#endif
