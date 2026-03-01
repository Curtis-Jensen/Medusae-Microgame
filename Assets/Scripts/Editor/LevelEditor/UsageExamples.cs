using UnityEngine;
using UnityEditor;

namespace LevelEditor
{
    /// <summary>
    /// USAGE EXAMPLES - How to use the Level Editor System
    /// </summary>
    public class LevelEditorUsageExample : MonoBehaviour
    {
        // ==================== BASIC WORKFLOW ====================
        /*
        
        STEP 1: Create a Level Asset
        ----------------------------
        LevelData level = LevelManager.CreateNewLevel("Assets/Levels/Tutorial.asset");
        This creates a new ScriptableObject-based asset that persists on disk.

        
        STEP 2: Place Objects in the Scene
        -----------------------------------
        1. Create prefabs of your game objects (enemies, obstacles, etc.)
        2. Place them in the scene manually
        3. Add the PlacedObject component to each instance
        
        
        STEP 3: Save the Level
        ----------------------
        LevelManager.SaveLevel();
        
        This syncs all scene objects back to the LevelData asset:
        - Reads all PlacedObject components in the scene
        - Extracts their transform data (position, rotation, scale)
        - Writes to LevelData.placedObjects list
        - Marks the asset dirty and saves via AssetDatabase
        - Persists to disk - survives Play Mode exit!

        
        STEP 4: Load the Level Back
        ----------------------------
        LevelManager.LoadLevel("Assets/Levels/Tutorial.asset");
        LevelManager.LoadLevelIntoScene();
        
        This:
        - Reads the LevelData asset from disk
        - Instantiates all saved objects into the scene
        - Positions them exactly as they were saved
        - Adds PlacedObject components for tracking

        
        STEP 5: Edit Mode + Play Mode Workflow
        ----------------------------------------
        Edit Mode:
        - Place an enemy at position (5, 0, 3)
        - Exit Play Mode
        - Position persists because it's saved to LevelData
        
        Play Mode:
        - Modify enemy position to (5, 5, 3)
        - Save BEFORE exiting: LevelManager.SaveLevel()
        - Position change persists to the asset
        
        */

        // ==================== CODE EXAMPLES ====================

        /// <summary>
        /// Example 1: Initialize a new level
        /// </summary>
        public static void Example_CreateNewLevel()
        {
            string levelPath = "Assets/Levels/MyLevel.asset";
            LevelData level = LevelManager.CreateNewLevel(levelPath);
            level.levelName = "Castle Siege";
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Example 2: Place multiple objects and save
        /// </summary>
        public static void Example_PlaceAndSave()
        {
            // Load the level
            LevelData level = LevelManager.LoadLevel("Assets/Levels/Tutorial.asset");

            // Suppose we have these in the scene (placed manually):
            // - Enemy prefab at (10, 0, 5)
            // - Obstacle prefab at (15, 0, 10)
            // Each should have PlacedObject component

            // Save everything to the asset
            LevelManager.SaveLevel();
            // Now the positions are persisted to disk!
        }

        /// <summary>
        /// Example 3: Load level and display objects
        /// </summary>
        public static void Example_LoadAndInspect()
        {
            LevelData level = LevelManager.LoadLevel("Assets/Levels/Tutorial.asset");

            Debug.Log($"Level: {level.levelName}");
            Debug.Log($"Objects: {level.placedObjects.Count}");

            foreach (var obj in level.placedObjects)
            {
                Debug.Log($"  - {obj.prefabPath} at {obj.position}");
            }
        }

        /// <summary>
        /// Example 4: Modify objects at runtime and save
        /// </summary>
        public static void Example_RuntimeModification()
        {
            // In Play Mode, you can modify scene objects
            if (Application.isPlaying)
            {
                // Find an enemy in the scene
                PlacedObject enemy = FindObjectOfType<PlacedObject>();

                if (enemy != null)
                {
                    // Move it
                    enemy.transform.position = new Vector3(20, 0, 15);

                    // Before exiting Play Mode, save to persist the change
                    LevelManager.SaveLevel();
                    // The new position is now in LevelData!
                }
            }
        }

        /// <summary>
        /// Example 5: Clear and reload a level
        /// </summary>
        public static void Example_ResetLevel()
        {
            LevelData level = LevelManager.LoadLevel("Assets/Levels/Tutorial.asset");

            // Clear the scene
            LevelManager.LoadLevelIntoScene(); // Clears old objects and reloads
        }

        /// <summary>
        /// Example 6: Programmatic level creation
        /// </summary>
        public static void Example_CreateProgrammatically()
        {
            LevelData level = LevelManager.CreateNewLevel("Assets/Levels/Generated.asset");

            // Add objects programmatically
            var skeleton = new PlacedObjectData
            {
                id = level.GenerateObjectId(),
                prefabPath = "Assets/Prefabs/Skeleton.prefab",
                position = new Vector3(0, 0, 0),
                rotation = Quaternion.identity,
                scale = Vector3.one
            };

            var goblin = new PlacedObjectData
            {
                id = level.GenerateObjectId(),
                prefabPath = "Assets/Prefabs/Goblin.prefab",
                position = new Vector3(5, 0, 0),
                rotation = Quaternion.identity,
                scale = Vector3.one
            };

            level.placedObjects.Add(skeleton);
            level.placedObjects.Add(goblin);

            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created level with {level.placedObjects.Count} objects");
        }
    }

    /// <summary>
    /// ARCHITECTURE OVERVIEW
    /// </summary>
    /*
    
    ┌─────────────────────────────────────────┐
    │       LEVEL DATA ASSET (Disk)           │
    │   ScriptableObject with PlacedObjects   │
    │   - Persists between Play Mode exits    │
    │   - Source of truth                     │
    └──────────────┬──────────────────────────┘
                   │
                   │ SaveLevel()
                   ├─→ Reads PlacedObject components in scene
                   ├─→ Extracts transform data
                   ├─→ Writes to LevelData.placedObjects
                   └─→ AssetDatabase.SaveAssets()
                   │
                   │ LoadLevelIntoScene()
                   ├─→ Clears old PlacedObjects from scene
                   ├─→ Reads LevelData.placedObjects
                   ├─→ Instantiates prefabs at saved positions
                   └─→ Adds PlacedObject components for tracking
                   │
    ┌──────────────▼──────────────────────────┐
    │     SCENE OBJECTS                       │
    │   (Edit Mode + Play Mode)               │
    │   Each has PlacedObject component       │
    │   containing:                           │
    │   - Object ID (unique)                  │
    │   - Prefab path (for recreation)        │
    │   - Current transform (synced on save)  │
    └─────────────────────────────────────────┘
    
    KEY POINTS:
    -----------
    1. LevelData is the single source of truth
    2. PlacedObject components in scene track object ID and prefab path
    3. SaveLevel() pulls data from scene into LevelData
    4. LoadLevelIntoScene() pushes data from LevelData to scene
    5. AssetDatabase.SaveAssets() persists everything to disk
    6. Works across Play Mode transitions
    
    */
}
