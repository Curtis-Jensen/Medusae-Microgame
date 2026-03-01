# Level Editor System - Quick Start

## What You Have

A minimal, clean architecture for a **manual-save level editor** that persists data to ScriptableObject assets.

### Files Created:

1. **LevelData.cs** - ScriptableObject defining level structure
2. **LevelManager.cs** - Static manager for load/save operations  
3. **PlacedObject.cs** - Component for tracking scene objects
4. **LevelEditorWindow.cs** - Editor UI for common operations
5. **UsageExamples.cs** - Code examples and architecture diagram

---

## Quick Start (5 minutes)

### 1. Open Level Editor Window
```
Menu → Window → Level Editor
```

### 2. Create a New Level
```
Set path: Assets/Levels/MyLevel.asset
Click: Create New Level
```

### 3. Place Objects in Scene
- Create/place prefab instances manually
- Select each → Add Component → PlacedObject
- Position them as desired

### 4. Save
```
Click: SAVE LEVEL (in editor window)
```

The level asset now contains all object positions, rotations, and scales on disk.

### 5. Exit and Reload
- Close Play Mode or exit editor
- Objects still in asset (persistent!)
- Reload anytime:
  ```
  Click: Load into Scene
  ```

---

## How It Works

### The Data Flow

**Save:** Scene → PlacedObject components → LevelData asset (disk)
```csharp
LevelManager.SaveLevel()
// 1. Finds all PlacedObject components in scene
// 2. Reads their transforms
// 3. Writes to LevelData.placedObjects
// 4. AssetDatabase.SaveAssets() → persists to disk
```

**Load:** LevelData asset → Instantiate prefabs → PlacedObject components
```csharp
LevelManager.LoadLevelIntoScene()
// 1. Reads LevelData.placedObjects from disk
// 2. Instantiates each prefab at saved position
// 3. Adds PlacedObject component for tracking
```

### Persistence

- **LevelData** is a ScriptableObject → saved to disk automatically
- **PlacedObject** components are scene objects → destroyed on Play Mode exit
- **Solution:** SaveLevel() before exiting Play Mode persists ALL data to the asset
- Next scene load: LoadLevelIntoScene() recreates everything from the asset

---

## Code Examples

### Basic Save/Load
```csharp
// Load
LevelData level = LevelManager.LoadLevel("Assets/Levels/Tutorial.asset");

// Edit scene...

// Save
LevelManager.SaveLevel();

// Load back anytime
LevelManager.LoadLevelIntoScene();
```

### Programmatic Level Creation
```csharp
LevelData level = LevelManager.CreateNewLevel("Assets/Levels/Auto.asset");

// Add objects
var enemy = new PlacedObjectData
{
    id = level.GenerateObjectId(),
    prefabPath = "Assets/Prefabs/Enemy.prefab",
    position = new Vector3(5, 0, 0),
    rotation = Quaternion.identity,
    scale = Vector3.one
};

level.placedObjects.Add(enemy);
EditorUtility.SetDirty(level);
AssetDatabase.SaveAssets();
```

### Runtime Modification + Save
```csharp
// In Play Mode
PlacedObject obj = FindObjectOfType<PlacedObject>();
obj.transform.position = new Vector3(10, 5, 20);

// Save before exiting
LevelManager.SaveLevel();
// New position persists!
```

---

## Architecture

```
LevelData Asset (Disk)
  ↕ SaveLevel() / LoadLevelIntoScene()
Scene Objects with PlacedObject Components
```

**Key principle:** LevelData is the source of truth. Everything syncs to/from it.

---

## Features Included

✅ **Persistent Save** - SaveLevel() uses AssetDatabase  
✅ **Play Mode Safe** - Data persists across Play Mode transitions  
✅ **Edit + Play Sync** - Both modes modify the same LevelData  
✅ **Unique IDs** - Each object tracked by ID  
✅ **Full Transform** - Position, rotation, scale preserved  
✅ **Prefab Tracking** - Stores prefab paths for recreation  
✅ **Editor Window** - Simple UI for common operations  
✅ **Source of Truth** - Single asset file defines the level  

---

## Next Steps (Autosave)

When ready to add autosave:
1. Create an EditorApplication callback
2. Save on SceneHierarchyHooks or timer
3. Call LevelManager.SaveLevel() periodically

```csharp
EditorApplication.update += () => {
    if (timeSinceLastSave > autoSaveInterval) {
        LevelManager.SaveLevel();
        timeSinceLastSave = 0;
    }
};
```

---

## Troubleshooting

**Objects disappear after Play Mode exit?**
- Call `LevelManager.SaveLevel()` before exiting Play Mode

**Asset not saving?**
- Check path exists and is in Assets/
- AssetDatabase requires paths to start with "Assets/"

**Prefab path empty?**
- PlacedObject.UpdatePrefabPath() is called in OnValidate()
- May need to manually assign or re-select the object

---

**Happy editing!** 🎮
