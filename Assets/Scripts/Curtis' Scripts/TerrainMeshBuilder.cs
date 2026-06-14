using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainMeshBuilder : MonoBehaviour
{
    [Header("Dimensions")]
    public int width = 50;
    public int depth = 50;
    [Tooltip("Number of vertices per unit — higher = smoother terrain, heavier mesh")]
    [Range(1, 4)]
    public int resolution = 1;

    [Header("Height")]
    public float heightScale = 5f;
    [Tooltip("Lower = broad rolling hills, higher = tight jagged noise")]
    public float noiseScale = 0.1f;
    public float noiseOffsetX = 0f;
    public float noiseOffsetZ = 0f;

    [Header("Seed")]
    [Tooltip("0 = random each time, any other value = deterministic")]
    public int seed = 0;

    MeshFilter meshFilter;
    MeshCollider meshCollider;

    // -------------------------------------------------------
    // Awake()
    // Caches components. Called once before Start.
    // -------------------------------------------------------
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    // -------------------------------------------------------
    // Start()
    // Builds the terrain mesh on scene load.
    // -------------------------------------------------------
    void Start()
    {
        BuildTerrain();
    }

    // -------------------------------------------------------
    // BuildTerrain()
    // Main entry point — resolves seed, generates and assigns mesh.
    // Called from Start and from the editor button.
    // -------------------------------------------------------
    public void BuildTerrain()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();

        float offsetX = noiseOffsetX;
        float offsetZ = noiseOffsetZ;

        if (seed != 0)
        {
            var rng = new System.Random(seed);
            offsetX = (float)(rng.NextDouble() * 10000);
            offsetZ = (float)(rng.NextDouble() * 10000);
        }
        else
        {
            offsetX = Random.Range(0f, 10000f);
            offsetZ = Random.Range(0f, 10000f);
        }

        Mesh mesh = GenerateMesh(offsetX, offsetZ);
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    // -------------------------------------------------------
    // GenerateMesh(offsetX, offsetZ)
    // Builds a grid mesh where each vertex Y is sampled from Perlin noise.
    // Returns the completed Mesh asset.
    // -------------------------------------------------------
    Mesh GenerateMesh(float offsetX, float offsetZ)
    {
        int cols = width * resolution + 1;
        int rows = depth * resolution + 1;
        float step = 1f / resolution;

        var vertices = new Vector3[cols * rows];
        var uvs = new Vector2[vertices.Length];
        var triangles = new int[(cols - 1) * (rows - 1) * 6];

        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                float wx = x * step;
                float wz = z * step;
                float y = Mathf.PerlinNoise(wx * noiseScale + offsetX,
                                            wz * noiseScale + offsetZ) * heightScale;

                int i = z * cols + x;
                vertices[i] = new Vector3(wx, y, wz);
                uvs[i] = new Vector2((float)x / (cols - 1), (float)z / (rows - 1));
            }
        }

        int t = 0;
        for (int z = 0; z < rows - 1; z++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                int bl = z * cols + x;
                int br = bl + 1;
                int tl = bl + cols;
                int tr = tl + 1;

                triangles[t++] = bl;
                triangles[t++] = tl;
                triangles[t++] = tr;

                triangles[t++] = bl;
                triangles[t++] = tr;
                triangles[t++] = br;
            }
        }

        var mesh = new Mesh { name = "ProceduralTerrain" };
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // supports large grids
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
