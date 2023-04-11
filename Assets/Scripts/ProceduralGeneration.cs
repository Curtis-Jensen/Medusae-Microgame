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
    [Tooltip("If true: objects spawn on spawn points. if false: objects spawn within a region")]
    public bool spawnPointBased = false;

    [Header("Dimensions")]
    [Tooltip("How far, multiplied by 10, the walls will go.")]
    public int widthMin = -5;
    [Tooltip("How far, multiplied by 10, the walls will go.")]
    public int widthMax = 6;
    [Tooltip("How far, multiplied by 10, the walls will go.")]
    public int lengthMin = 15;
    [Tooltip("How far, multiplied by 10, the walls will go.")]
    public int lengthMax = 26;
    [Tooltip("How far the walls will tilt")]
    public float tiltAngle = 10;
    [Range(0, 1)]
    [Tooltip("What percentage of the map will be filled with walls")]
    public float objectPrefabChance;

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

        if (spawnPointBased) PlaceObjectsBySpawnPoints(currentObjectPrefabChance);
        else PlaceObjectsInRegion(currentObjectPrefabChance);

        floor.BuildNavMesh(); // 50
    }

    /* 10 Deletes old map if there is one just in case this is called multiple times
     * 
     * 20 Make an array of +100 walls.
     * Currently hardcoded to make a grid of walls that fits the current square map
     * 
     * 30 Random chance that they don’t show up
     *     
     * 40 Random rotation
     */
    /// <summary>
    /// Places objects based off the region that is specified
    /// </summary>
    void PlaceObjectsInRegion(float currentObjectPrefabChance)
    {
        var children = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 1; i < children.Length; i++) // 10
            DestroyImmediate(children[i].gameObject);

        for (int x = widthMin; x < widthMax; x++) // 20
            for (int z = lengthMin; z < lengthMax; z++)
            {
                if (Random.value > currentObjectPrefabChance) continue; // 30

                var position = new Vector3(x * 10, 2, z * 10);
                var rotation =
                    new Vector3(Random.Range(-tiltAngle, tiltAngle), Random.Range(0, 360), Random.Range(-tiltAngle, tiltAngle)); // 40
                var newPrefab = Instantiate(objectPrefab, position, Quaternion.Euler(rotation), gameObject.transform);

                if (stretchy)
                    newPrefab.transform.localScale = new Vector3(Random.Range(1, stretchAmounts), Random.Range(1, stretchAmounts), Random.Range(1, stretchAmounts));
            }
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
            var children = new List<Transform>();
            for (int i = spawnPoint.childCount - 1; i >= 0; i--)
            {
                var child = spawnPoint.GetChild(i);
                if (child != null)
                {
                    children.Add(child);
                }
            }

            // Destroy the children
            foreach (Transform child in children)
            {
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
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
