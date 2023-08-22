using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class ProceduralGeneration : MonoBehaviour
{
    [Header("General")]
    [Tooltip("The floor needs to be kept in mind so the nav meshes can be baked.")]
    public NavMeshSurface floor;
    [Tooltip("The walls, trees, or other objects that will be sprinkled around")]
    public GameObject[] objectPrefabs;
    [Range(0, 1)]
    [Tooltip("The chance that the objects will spawn as much as possible, making a maze")]
    public float packedMazeChance;

    [Header("Dimensions")]
    [Range(0, 1)]
    [Tooltip("What percentage of the map will be filled with walls")]
    public float spawnChance;
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

    /* 🤔 These three lines of code may seem overly complex,
     * but we need to be sure that the spawnChance goes back to what it was before being packed
     */
    public void CreateMap()
    {
        float spawnChance; // 🤔
        if (Random.value < packedMazeChance) spawnChance = 1;
        else spawnChance = this.spawnChance;

        SpawnObjects(spawnChance);

        floor.BuildNavMesh(); // Bake navigation map
    }

    protected abstract void SpawnObjects(float spawnChance);
}
