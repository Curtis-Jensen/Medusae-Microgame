using UnityEngine;
using UnityEngine.AI;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshLink))]
    class NavMeshLinkEditor : Editor
    {
        SerializedProperty agentTypeID;
        SerializedProperty area;
        SerializedProperty costModifier;
        SerializedProperty autoUpdatePosition;
        SerializedProperty bidirectional;
        SerializedProperty endPoint;
        SerializedProperty startPoint;
        SerializedProperty width;

        static int s_SelectedID;
        static int s_SelectedPoint = -1;

        static Color s_HandleColor = new Color(255f, 167f, 39f, 210f) / 255;
        static Color s_HandleColorDisabled = new Color(255f * 0.75f, 167f * 0.75f, 39f * 0.75f, 100f) / 255;

        void OnEnable()
        {
            agentTypeID = serializedObject.FindProperty("agentTypeID");
            area = serializedObject.FindProperty("area");
            costModifier = serializedObject.FindProperty("costModifier");
            autoUpdatePosition = serializedObject.FindProperty("autoUpdatePosition");
            bidirectional = serializedObject.FindProperty("bidirectional");
            endPoint = serializedObject.FindProperty("endPoint");
            startPoint = serializedObject.FindProperty("startPoint");
            width = serializedObject.FindProperty("width");

            s_SelectedID = 0;
            s_SelectedPoint = -1;

            NavMeshVisualizationSettings.showNavigation++;
        }

        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        static Matrix4x4 UnscaledLocalToWorldMatrix(Transform t)
        {
            return Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
        }

        void AlignTransformToEndPoints(NavMeshLink navLink)
        {
            var mat = UnscaledLocalToWorldMatrix(navLink.transform);

            var worldStartPt = mat.MultiplyPoint(navLink.startPoint);
            var worldEndPt = mat.MultiplyPoint(navLink.endPoint);

            var forward = worldEndPt - worldStartPt;
            var up = navLink.transform.up;

            // Flatten
            forward -= Vector3.Dot(up, forward) * up;

            var transform = navLink.transform;
            transform.rotation = Quaternion.LookRotation(forward, up);
            transform.position = (worldEndPt + worldStartPt) * 0.5f;
            transform.localScale = Vector3.one;

            navLink.startPoint = transform.InverseTransformPoint(worldStartPt);
            navLink.endPoint = transform.InverseTransformPoint(worldEndPt);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", agentTypeID);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(startPoint);
            EditorGUILayout.PropertyField(endPoint);

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);
            if (GUILayout.Button("Swap"))
            {
                foreach (NavMeshLink navLink in targets)
                {
                    var tmp = navLink.startPoint;
                    navLink.startPoint = navLink.endPoint;
                    navLink.endPoint = tmp;
                }
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Align Transform"))
            {
                foreach (NavMeshLink navLink in targets)
                {
                    Undo.RecordObject(navLink.transform, "Align Transform to End Points");
                    Undo.RecordObject(navLink, "Align Transform to End Points");
                    AlignTransformToEndPoints(navLink);
                }
                SceneView.RepaintAll();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(width);
            EditorGUILayout.PropertyField(costModifier);
            EditorGUILayout.PropertyField(autoUpdatePosition);
            EditorGUILayout.PropertyField(bidirectional);

            NavMeshComponentsGUIUtility.AreaPopup("Area Type", area);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
        }

        static Vector3 CalcLinkRight(NavMeshLink navLink)
        {
            var dir = navLink.endPoint - navLink.startPoint;
            return (new Vector3(-dir.z, 0.0f, dir.x)).normalized;
        }

        static void DrawLink(NavMeshLink navLink)
        {
            var right = CalcLinkRight(navLink);
            var rad = navLink.width * 0.5f;

            Gizmos.DrawLine(navLink.startPoint - right * rad, navLink.startPoint + right * rad);
            Gizmos.DrawLine(navLink.endPoint - right * rad, navLink.endPoint + right * rad);
            Gizmos.DrawLine(navLink.startPoint - right * rad, navLink.endPoint - right * rad);
            Gizmos.DrawLine(navLink.startPoint + right * rad, navLink.endPoint + right * rad);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
        static void RenderBoxGizmo(NavMeshLink navLink, GizmoType gizmoType)
        {
            if (!EditorApplication.isPlaying)
                navLink.UpdateLink();

            var color = s_HandleColor;
            if (!navLink.enabled)
                color = s_HandleColorDisabled;

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            Gizmos.matrix = UnscaledLocalToWorldMatrix(navLink.transform);

            Gizmos.color = color;
            DrawLink(navLink);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navLink.transform.position, "NavMeshLink Icon", true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void RenderBoxGizmoNotSelected(NavMeshLink navLink, GizmoType gizmoType)
        {
            if (NavMeshVisualizationSettings.showNavigation > 0)
            {
                var color = s_HandleColor;
                if (!navLink.enabled)
                    color = s_HandleColorDisabled;

                var oldColor = Gizmos.color;
                var oldMatrix = Gizmos.matrix;

                Gizmos.matrix = UnscaledLocalToWorldMatrix(navLink.transform);

                Gizmos.color = color;
                DrawLink(navLink);

                Gizmos.matrix = oldMatrix;
                Gizmos.color = oldColor;
            }

            Gizmos.DrawIcon(navLink.transform.position, "NavMeshLink Icon", true);
        }

        public void OnSceneGUI()
        {
            var navLink = (NavMeshLink)target;
            if (!navLink.enabled)
                return;

            var mat = UnscaledLocalToWorldMatrix(navLink.transform);

            var startPt = mat.MultiplyPoint(navLink.startPoint);
            var endPt = mat.MultiplyPoint(navLink.endPoint);
            var midPt = Vector3.Lerp(startPt, endPt, 0.35f);
            var startSize = HandleUtility.GetHandleSize(startPt);
            var endSize = HandleUtility.GetHandleSize(endPt);
            var midSize = HandleUtility.GetHandleSize(midPt);

            var zup = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
            var right = mat.MultiplyVector(CalcLinkRight(navLink));

            var oldColor = Handles.color;
            Handles.color = s_HandleColor;

            Vector3 pos;

            if (navLink.GetInstanceID() == s_SelectedID && s_SelectedPoint == 0)
            {
                EditorGUI.BeginChangeCheck();
                Handles.CubeHandleCap(0, startPt, zup, 0.1f * startSize, Event.current.type);
                pos = Handles.PositionHandle(startPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navLink, "Move link point");
                    navLink.startPoint = mat.inverse.MultiplyPoint(pos);
                }
            }
            else
            {
                if (Handles.Button(startPt, zup, 0.1f * startSize, 0.1f * startSize, Handles.CubeHandleCap))
                {
                    s_SelectedPoint = 0;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }

            if (navLink.GetInstanceID() == s_SelectedID && s_SelectedPoint == 1)
            {
                EditorGUI.BeginChangeCheck();
                Handles.CubeHandleCap(0, endPt, zup, 0.1f * startSize, Event.current.type);
                pos = Handles.PositionHandle(endPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navLink, "Move link point");
                    navLink.endPoint = mat.inverse.MultiplyPoint(pos);
                }
            }
            else
            {
                if (Handles.Button(endPt, zup, 0.1f * endSize, 0.1f * endSize, Handles.CubeHandleCap))
                {
                    s_SelectedPoint = 1;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }

            EditorGUI.BeginChangeCheck();
            pos = Handles.Slider(midPt + right * navLink.width * 0.5f, right, midSize * 0.03f, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(navLink, "Adjust link width");
                navLink.width = Mathf.Max(0.0f, 2.0f * Vector3.Dot(right, (pos - midPt)));
            }

            EditorGUI.BeginChangeCheck();
            pos = Handles.Slider(midPt - right * navLink.width * 0.5f, -right, midSize * 0.03f, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(navLink, "Adjust link width");
                navLink.width = Mathf.Max(0.0f, 2.0f * Vector3.Dot(-right, (pos - midPt)));
            }

            Handles.color = oldColor;
        }

        [MenuItem("GameObject/AI/NavMesh Link", false, 2002)]
        static public void CreateNavMeshLink(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            GameObject go = NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Link", parent);
            go.AddComponent<NavMeshLink>();
            var view = SceneView.lastActiveSceneView;
            if (view != null)
                view.MoveToView(go.transform);
        }
    }
}
