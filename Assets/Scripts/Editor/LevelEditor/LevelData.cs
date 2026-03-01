using UnityEngine;
using System.Collections.Generic;

namespace LevelEditor
{
    /// <summary>
    /// Serializable container for a placed prefab instance
    /// </summary>
    [System.Serializable]
    public class PlacedObjectData
    {
        public string prefabPath;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale = Vector3.one;
        public int id; // Unique identifier for the instance
    }

    /// <summary>
    /// Level data asset - the source of truth for all level information
    /// </summary>
    public class LevelData : ScriptableObject
    {
        [SerializeField]
        public List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();

        [SerializeField]
        public string levelName = "Untitled Level";

        [SerializeField]
        public Vector3 playerSpawnPosition = Vector3.zero;

        [SerializeField]
        public int nextObjectId = 1;

        /// <summary>
        /// Generate a new unique ID for placed objects
        /// </summary>
        public int GenerateObjectId()
        {
            return nextObjectId++;
        }

        /// <summary>
        /// Find a placed object by its ID
        /// </summary>
        public PlacedObjectData GetObjectById(int id)
        {
            return placedObjects.Find(obj => obj.id == id);
        }

        /// <summary>
        /// Remove a placed object by ID
        /// </summary>
        public bool RemoveObjectById(int id)
        {
            return placedObjects.RemoveAll(obj => obj.id == id) > 0;
        }

        /// <summary>
        /// Clear all placed objects
        /// </summary>
        public void Clear()
        {
            placedObjects.Clear();
            nextObjectId = 1;
        }
    }
}
