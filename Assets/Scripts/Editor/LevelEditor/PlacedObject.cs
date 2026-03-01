using UnityEditor;
using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Component attached to scene objects placed via the level editor
    /// Allows syncing transform data back to LevelData
    /// </summary>
    public class PlacedObject : MonoBehaviour
    {
        [SerializeField]
        private int objectId = -1;

        [SerializeField]
        private string prefabPath;

        private void OnValidate()
        {
            // Auto-detect prefab path if not set
            if (string.IsNullOrEmpty(prefabPath))
            {
                UpdatePrefabPath();
            }
        }

        /// <summary>
        /// Set the unique ID for this placed object
        /// </summary>
        public void SetObjectId(int id)
        {
            objectId = id;
        }

        /// <summary>
        /// Get the unique ID
        /// </summary>
        public int GetObjectId()
        {
            return objectId;
        }

        /// <summary>
        /// Update the prefab path (called when placed or moved)
        /// </summary>
        public void UpdatePrefabPath()
        {
            //There are some compiler errors and i just want to play real quick


            // string assetPath = PrefabUtility.GetPrefabAssetPath(gameObject);
            // if (assetPath != null)
            // {
            //     prefabPath = assetPath;
            // }
        }

        /// <summary>
        /// Serialize this object's transform to PlacedObjectData
        /// </summary>
        public PlacedObjectData GetObjectData()
        {
            return new PlacedObjectData
            {
                id = objectId,
                prefabPath = prefabPath,
                position = transform.position,
                rotation = transform.rotation,
                scale = transform.localScale
            };
        }

        /// <summary>
        /// Restore this object's transform from PlacedObjectData
        /// </summary>
        public void SetObjectData(PlacedObjectData data)
        {
            objectId = data.id;
            prefabPath = data.prefabPath;
            transform.position = data.position;
            transform.rotation = data.rotation;
            transform.localScale = data.scale;
        }
    }
}
