using UnityEngine.AI;


    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifier))]
    class NavMeshModifierEditor : Editor
    {
        SerializedProperty affectedAgents;
        SerializedProperty area;
        SerializedProperty ignoreFromBuild;
        SerializedProperty overrideArea;

        void OnEnable()
        {
            affectedAgents = serializedObject.FindProperty("affectedAgents");
            area = serializedObject.FindProperty("area");
            ignoreFromBuild = serializedObject.FindProperty("ignoreFromBuild");
            overrideArea = serializedObject.FindProperty("overrideArea");

            NavMeshVisualizationSettings.showNavigation++;
        }

        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(ignoreFromBuild);

            EditorGUILayout.PropertyField(overrideArea);
            if (overrideArea.boolValue)
            {
                EditorGUI.indentLevel++;
                NavMeshComponentsGUIUtility.AreaPopup("Area Type", area);
                EditorGUI.indentLevel--;
            }

            NavMeshComponentsGUIUtility.AgentMaskPopup("Affected Agents", affectedAgents);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }

