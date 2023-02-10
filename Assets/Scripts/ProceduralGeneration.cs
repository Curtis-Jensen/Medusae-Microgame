using UnityEngine;
using UnityEngine.AI;

public class ProceduralGeneration : MonoBehaviour
{
    [Header("Inputs")]
    [Tooltip("The floor needs to be kept in mind so the nav meshes can be baked.")]
    public NavMeshSurface floor;

    [Tooltip("The wall that will be sprinkled around")]
    public GameObject wallPrefab;

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
    public float wallChance;

    [Header("Forest")]
    [Tooltip("Whether forestey behaviors such as stretching will occur")]
    public bool forest;
    [Tooltip("How much the prefabs can be stretched in any direction")]
    [Min(1)]
    public float stretchAmounts = 1;

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
     * 50 Bake navigation map
     */
    public void CreateMap()
    {
        var children = gameObject.GetComponentsInChildren<Transform>();

        for (int i = 1; i < children.Length; i++) // 10
            DestroyImmediate(children[i].gameObject);

        for (int x = widthMin; x < widthMax; x++) // 20
            for (int z = lengthMin; z < lengthMax; z++)
            {
                if (Random.value > wallChance) continue; // 30

                var position = new Vector3(x * 10, 2, z * 10);
                var rotation = 
                    new Vector3(Random.Range(-tiltAngle, tiltAngle), Random.Range(0, 360), Random.Range(-tiltAngle, tiltAngle)); // 40
                var newPrefab = Instantiate(wallPrefab, position, Quaternion.Euler(rotation), gameObject.transform);

                if(forest)
                    newPrefab.transform.localScale = new Vector3(Random.Range(1, stretchAmounts), Random.Range(1, stretchAmounts), Random.Range(1, stretchAmounts));
            }

        floor.BuildNavMesh(); // 50
    }
}
