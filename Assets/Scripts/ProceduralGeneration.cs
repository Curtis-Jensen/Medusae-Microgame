using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProceduralGeneration : MonoBehaviour
{
    [Header("General")]
    [Tooltip("The floor needs to be kept in mind so the nav meshes can be baked.")]
    public NavMeshSurface floor;
    [Tooltip("The wall, tree, or other object that will be sprinkled around")]
    public GameObject objectPrefab;
    [Range(0, 1)]
    [Tooltip("The chance that the objects will spawn as much as possible, making a maze")]
    public float packedMazeChance;

    [Header("Dimensions")]
    [Range(0, 1)]
    [Tooltip("What percentage of the map will be filled with walls")]
    public float objectPrefabChance;
    [Tooltip("How far the walls will tilt")]
    public float tiltAngle = 10;

    [Header("Forest")]
    [Tooltip("Whether stretching will occur")]
    public bool stretchy;
    [Tooltip("How much the prefabs can be stretched in any direction")]
    [Min(1)]
    public float stretchAmounts = 1;

    void Start()
    {
        CreateMap();
    }

    /* 
     * 
     * 50 Bake navigation map
     */
    public void CreateMap()
    {
        float currentObjectPrefabChance;
        if (Random.value < packedMazeChance) currentObjectPrefabChance = 1;
        else currentObjectPrefabChance = objectPrefabChance;

        PlaceObjectsBySpawnPoints(currentObjectPrefabChance);

        floor.BuildNavMesh(); // 50
    }

    /*
        🌀 Get a list of all spawn points, defined as children of this game object

        🧨 For each spawn point, delete any existing walls and spawn a new object if the random value is below the prefab chance

        💥 Delete existing walls from spawn points immediately so it can be used in the inspector

        🧱 Spawn new prefab with the spawnpoint's position, the generated rotation, and with the spawn point as a parent
        
        🎉 Instantiate the prefab with the spawn point as its parent

        🎨 Stretch the prefab if the stretchy flag is set
     */
    /// <summary>
    /// Places the objects based off the spawn points that are laid out
    /// </summary>
    /// <param name="currentObjectPrefabChance">The chance of spawning a new object on each spawn point</param>
    void PlaceObjectsBySpawnPoints(float currentObjectPrefabChance)
    {
        var spawnPoints = new List<Transform>(); // 🌀
        foreach (Transform child in transform)
        {
            if (child.CompareTag("SpawnPoint"))
                spawnPoints.Add(child);
        }

        foreach (var spawnPoint in spawnPoints) // 🧨
        {
            for (int i = spawnPoint.childCount - 1; i >= 0; i--) // 💥
            {
                var child = spawnPoint.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            if (Random.value <= currentObjectPrefabChance)
            {
                var rotation = Quaternion.Euler(Random.Range(-tiltAngle, tiltAngle), // 🧱
                                                Random.Range(0, 360),
                                                Random.Range(-tiltAngle, tiltAngle));

                var newWall = Instantiate(objectPrefab, spawnPoint.position, rotation, spawnPoint); // 🎉

                if (stretchy)
                    newWall.transform.localScale = new Vector3(Random.Range(1, stretchAmounts), // 🎨
                                                               Random.Range(1, stretchAmounts),
                                                               Random.Range(1, stretchAmounts));
            }
        }
    }
}
