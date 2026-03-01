using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace LevelEditor
{
    /// <summary>
    /// Core level editor manager - handles saving/loading and persistence
    /// Works in both Edit Mode and Play Mode
    /// </summary>
    public static class LevelManager
    {
        private static LevelData currentLevelData;

        /// <summary>
        /// Load level data from asset or create new
        /// </summary>
        public static LevelData LoadLevel(string assetPath)
        {
            currentLevelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);

            if (currentLevelData == null)
            {
                Debug.LogWarning($"Level asset not found at {assetPath}. Creating new level data.");
                return CreateNewLevel(assetPath);
            }

            return currentLevelData;
        }

        /// <summary>
        /// Create a new level data asset
        /// </summary>
        public static LevelData CreateNewLevel(string assetPath)
        {
            // Ensure the directory exists
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            currentLevelData = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(currentLevelData, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created new level at {assetPath}");
            return currentLevelData;
        }

        /// <summary>
        /// Save current level - persists scene objects to LevelData asset
        /// </summary>
        public static void SaveLevel()
        {
            if (currentLevelData == null)
            {
                Debug.LogError("No level loaded. Call LoadLevel() first.");
                return;
            }

            // Sync all scene objects to LevelData
            SyncSceneToLevelData();

            // Mark asset dirty and save
            EditorUtility.SetDirty(currentLevelData);
            AssetDatabase.SaveAssets();

            Debug.Log($"Level saved: {currentLevelData.levelName}");
        }

        /// <summary>
        /// Load level into the scene - instantiates all objects from LevelData
        /// </summary>
        public static void LoadLevelIntoScene()
        {
            if (currentLevelData == null)
            {
                Debug.LogError("No level loaded. Call LoadLevel() first.");
                return;
            }

            // Clear existing placed objects
            ClearSceneObjects();

            // Instantiate all objects from level data
            foreach (var objData in currentLevelData.placedObjects)
            {
                InstantiateObjectFromData(objData);
            }

            Debug.Log($"Loaded level into scene: {currentLevelData.levelName}");
        }

        /// <summary>
        /// Get the current loaded level data
        /// </summary>
        public static LevelData GetCurrentLevelData()
        {
            return currentLevelData;
        }

        // ==================== PRIVATE METHODS ====================

        /// <summary>
        /// Sync all scene objects with the PlacedObject component back to LevelData
        /// </summary>
        private static void SyncSceneToLevelData()
        {
            // Clear existing data
            currentLevelData.placedObjects.Clear();

            // Find all PlacedObject components in scene
            PlacedObject[] placedObjects = Object.FindObjectsOfType<PlacedObject>();

            foreach (var placedObject in placedObjects)
            {
                var data = placedObject.GetObjectData();
                currentLevelData.placedObjects.Add(data);

                // Ensure we track the highest ID
                if (data.id >= currentLevelData.nextObjectId)
                {
                    currentLevelData.nextObjectId = data.id + 1;
                }
            }
        }

        /// <summary>
        /// Instantiate an object in the scene from PlacedObjectData
        /// </summary>
        private static void InstantiateObjectFromData(PlacedObjectData data)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found at {data.prefabPath}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.position = data.position;
            instance.transform.rotation = data.rotation;
            instance.transform.localScale = data.scale;

            // Add or update PlacedObject component
            PlacedObject placedObject = instance.GetComponent<PlacedObject>();
            if (placedObject == null)
            {
                placedObject = instance.AddComponent<PlacedObject>();
            }
            placedObject.SetObjectId(data.id);
        }

        /// <summary>
        /// Clear all PlacedObject instances from the scene
        /// </summary>
        private static void ClearSceneObjects()
        {
            PlacedObject[] placedObjects = Object.FindObjectsOfType<PlacedObject>();
            foreach (var placedObject in placedObjects)
            {
                Object.DestroyImmediate(placedObject.gameObject);
            }
        }
    }
}
