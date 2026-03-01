using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace LevelEditor
{
    /// <summary>
    /// Example Editor Window showing how to use the Level System
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        private string levelAssetPath = "Assets/Levels/MyLevel.asset";
        private LevelData currentLevel;

        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Level Editor", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Asset path field
            levelAssetPath = EditorGUILayout.TextField("Level Asset Path", levelAssetPath);

            GUILayout.Space(10);

            // Load/Create buttons
            if (GUILayout.Button("Load Level", GUILayout.Height(30)))
            {
                currentLevel = LevelManager.LoadLevel(levelAssetPath);
            }

            if (GUILayout.Button("Create New Level", GUILayout.Height(30)))
            {
                currentLevel = LevelManager.CreateNewLevel(levelAssetPath);
            }

            GUILayout.Space(10);

            // Load into scene
            if (GUILayout.Button("Load into Scene", GUILayout.Height(30)))
            {
                if (currentLevel != null)
                {
                    LevelManager.LoadLevelIntoScene();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "No level loaded. Load a level first.", "OK");
                }
            }

            GUILayout.Space(10);

            // Save button
            GUI.color = Color.green;
            if (GUILayout.Button("SAVE LEVEL", GUILayout.Height(40)))
            {
                if (currentLevel != null)
                {
                    LevelManager.SaveLevel();
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "No level loaded. Load a level first.", "OK");
                }
            }
            GUI.color = Color.white;

            GUILayout.Space(20);

            // Current level info
            if (currentLevel != null)
            {
                EditorGUILayout.LabelField("Current Level", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Name", currentLevel.levelName);
                EditorGUILayout.LabelField("Placed Objects", currentLevel.placedObjects.Count.ToString());
            }
            else
            {
                EditorGUILayout.HelpBox("No level loaded", MessageType.Info);
            }
        }
    }
}
