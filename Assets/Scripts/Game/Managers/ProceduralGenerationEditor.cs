#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.Game
{

    [CustomEditor(typeof(ProceduralGeneration))]
    public class ProceduralGenerationEditor : Editor
    {
        /* 10 When the inspector renders it's GUI:
         * 
         * 20 Render the things that usually render
         */
        public override void OnInspectorGUI() // 10
        {
            DrawDefaultInspector(); // 20

            if (GUILayout.Button("Generate"))
            {
                var mapGenerator = (ProceduralGeneration)target;
                mapGenerator.CreateMap();
            }
        }
    }
}
#endif