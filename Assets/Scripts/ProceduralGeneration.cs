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

    /// <summary>
    /// Places the objects based off the spawn points that are laid out
    /// </summary>
    void PlaceObjectsBySpawnPoints(float currentObjectPrefabChance)
    {
        var spawnPoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("SpawnPoint"))
            {
                spawnPoints.Add(child);
            }
        }

        foreach (var spawnPoint in spawnPoints)
        {
            // Delete existing walls from previous spawns
            for (int i = spawnPoint.childCount - 1; i >= 0; i--)
            {
                var child = spawnPoint.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            // Spawn new wall
            if (Random.value <= currentObjectPrefabChance)
            {
                var position = spawnPoint.position;
                var rotation = Quaternion.Euler(Random.Range(-tiltAngle, tiltAngle),
                                                Random.Range(0, 360),
                                                Random.Range(-tiltAngle, tiltAngle));
                var newWall = Instantiate(objectPrefab, position, rotation, spawnPoint);

                if (stretchy)
                {
                    newWall.transform.localScale = new Vector3(Random.Range(1, stretchAmounts),
                                                                Random.Range(1, stretchAmounts),
                                                                Random.Range(1, stretchAmounts));
                }
            }
        }
    }
}
