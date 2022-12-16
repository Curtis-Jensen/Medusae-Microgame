using UnityEngine;
using UnityEngine.AI;

public class ProceduralGeneration : MonoBehaviour
{
    [Tooltip("The floor needs to be kept in mind so the nav meshes can be baked.")]
    public NavMeshSurface floor;

    [Tooltip("The wall that will be sprinkled around")]
    public GameObject wallPrefab;

    [Range(0, 1)]
    [Tooltip("What percentage of the map will be filled with walls")]
    public float wallChance;

    void Start()
    {
        CreateMap();
    }

    /* 10 Deletes old map if there is one just in case this is called multiple times
     * 
     * 20 Make an array of +100 walls.
     * Currently hardcoded to make a grid of walls that fits the current square map
     * 
     * 30 Random chance that they don’t show up
     *     
     * 40 Random rotation
     *   
     * 
     * Bake navigation map
     */
    public void CreateMap()
    {
        var children = gameObject.GetComponentsInChildren<Transform>();

        for (int i = 1; i < children.Length; i++) // 10
            DestroyImmediate(children[i].gameObject);

        for (int x = -5; x < 6; x++) // 20
            for (int z = 15; z < 26; z++)
            {
                if (Random.value > wallChance) continue; // 30

                var position = new Vector3(x * 10, 5, z * 10);
                var rotation = new Vector3(0, Random.Range(0, 360), 0); // 40
                Instantiate(wallPrefab, position, Quaternion.Euler(rotation), gameObject.transform);
            }

        floor.BuildNavMesh();
    }
}
