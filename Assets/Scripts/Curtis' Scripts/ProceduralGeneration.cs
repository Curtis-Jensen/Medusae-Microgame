using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProceduralGeneration : MonoBehaviour
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

    /*  🧨 For each spawn point, delete any existing walls and spawn a new object if the random value is below the prefab chance

        💥 Delete existing walls from spawn points immediately so it can be used in the inspector

        🧱 Spawn new prefab with the spawnpoint's position, the generated rotation, and with the spawn point as a parent
        
        🎉 Instantiate the prefab with the spawn point as its parent

        🎨 Stretch the prefab if the stretchy flag is set
     */
    /// <summary>
    /// Places the objects based off the spawn points that are laid out
    /// </summary>
    /// <param name="spawnChance">The chance of spawning a new object on each spawn point</param>
    void SpawnObjects(float spawnChance)
    {
        foreach (Transform spawnPoint in transform) // 🧨
        {
            for (int i = spawnPoint.childCount - 1; i >= 0; i--) // 💥
            {
                var child = spawnPoint.GetChild(i);

                if(child.CompareTag("Procedural"))
                    DestroyImmediate(child.gameObject);
            }

            if (Random.value >= spawnChance) continue;

            int randomIndex = Random.Range(0, objectPrefabs.Length);
            GameObject chosenObject = objectPrefabs[randomIndex];

            var rotation = Quaternion.Euler(Random.Range(-tiltAngle, tiltAngle), // 🧱
                                            Random.Range(0, 360),
                                            Random.Range(-tiltAngle, tiltAngle));

            var newObject = Instantiate(chosenObject, spawnPoint.position, rotation, spawnPoint); // 🎉

            if (stretchy)
                newObject.transform.localScale = new Vector3(Random.Range(1, stretchAmounts), // 🎨
                                                           Random.Range(1, stretchAmounts),
                                                           Random.Range(1, stretchAmounts));
        }
    }
}
