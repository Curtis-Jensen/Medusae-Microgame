using UnityEngine;

/// <summary>
/// Fortnite-style grid-based building system for walls and floors.
/// Attach this to the player and assign wall and floor prefabs.
/// </summary>
public class BuildingSystem : MonoBehaviour
{
    private enum BuildType { Wall, FloorBelow, FloorAbove }
    [Header("Building Settings")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private float gridSize = 2f; // Size of each grid cell
    [SerializeField] private float buildDistance = 10f; // How far the player can build
    [SerializeField] private float angleThreshold = 0.707f; // sin(45°), angle above which to treat as floor/ceiling
    [SerializeField] private float angleBuffer = 0.05f; // Buffer to prevent flickering near threshold
    
    [Header("Layer Masks")]
    [SerializeField] private LayerMask buildSurface = -1; // What surfaces can walls be placed on
    [SerializeField] private LayerMask buildingLayer; // What layer walls are on
    
    
    [Header("Materials")]
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material floorMaterial;
    
    private Camera playerCamera;
    private BuildingPreview buildingPreview;
    private bool canPlacePiece = true;
    private BuildType currentBuildType = BuildType.Wall;
    private int buildingResource = 100; // Current resources available
    
    private void Start()
    {
        playerCamera = GetComponentInParent<Camera>();
        buildingPreview = GetComponent<BuildingPreview>();
        buildingPreview.wallPrefab = wallPrefab;
        buildingPreview.floorPrefab = floorPrefab;
    }
    
    /// <summary>
    /// Determines what type of building to place based on camera direction
    /// Uses dot product to check if looking more than 45° above/below horizontal
    /// </summary>
    private BuildType DetermineBuildType()
    {
        Vector3 forward = playerCamera.transform.forward;
        float verticalComponent = Vector3.Dot(forward, Vector3.up);
        float threshold = angleThreshold + angleBuffer;
        
        if (verticalComponent > threshold)
            return BuildType.FloorAbove;
        else if (verticalComponent < -threshold)
            return BuildType.FloorBelow;
        
        return BuildType.Wall;
    }
    
    private void Update()
    {
        // Update build type based on camera direction
        currentBuildType = DetermineBuildType();
        
        // Update preview
        UpdatePreviewDisplay();
        
        // Build on left click
        if (Input.GetMouseButtonDown(0) && canPlacePiece)
        {
            PlaceBuilding();
        }
    }
    
    /// <summary>
    /// Updates the preview display based on current build type and placement validity
    /// </summary>
    private void UpdatePreviewDisplay()
    {
        Vector3 snappedPosition = GetPreviewPosition();
        canPlacePiece = IsPlacementValid(snappedPosition);
        
        buildingPreview.UpdatePreview((BuildingPreview.BuildType)currentBuildType, snappedPosition, canPlacePiece);
    }
    
    /// <summary>
    /// Gets the snapped position for the current build type
    /// </summary>
    private Vector3 GetPreviewPosition()
    {
        Vector3 previewPosition;
        
        if (currentBuildType == BuildType.Wall)
        {
            previewPosition = buildingPreview.GetWallPreviewPosition();
        }
        else
        {
            previewPosition = buildingPreview.GetFloorPreviewPosition(
                (BuildingPreview.BuildType)currentBuildType, 
                transform.position
            );
        }
        
        return SnapToGrid(previewPosition);
    }

    
    /// <summary>
    /// Snaps a position to the grid
    /// </summary>
    private Vector3 SnapToGrid(Vector3 position)
    {
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.y = Mathf.Round(position.y / gridSize) * gridSize;
        position.z = Mathf.Round(position.z / gridSize) * gridSize;
        return position;
    }
    
    /// <summary>
    /// Checks if a wall can be placed at this position
    /// </summary>
    private bool IsPlacementValid(Vector3 position)
    {        
        // Check if there's already a wall at this position
        Collider[] colliders = Physics.OverlapBox(
            position, 
            new Vector3(gridSize / 2f, gridSize / 2f, gridSize / 2f),
            Quaternion.identity,
            buildingLayer
        );
        
        if (colliders.Length > 0)
        {
            return false; // Wall already exists here
        }
        
        return true;
    }
    
    /// <summary>
    /// Places a building (wall or floor) at the current preview position
    /// </summary>
    private void PlaceBuilding()
    {
        Vector3 buildPosition = GetPreviewPosition();
        GameObject prefab = null;
        Material material = null;
        string buildingName = "";
        
        if (currentBuildType == BuildType.Wall)
        {
            prefab = wallPrefab;
            material = wallMaterial;
            buildingName = "Wall";
        }
        else if (currentBuildType == BuildType.FloorBelow || currentBuildType == BuildType.FloorAbove)
        {
            prefab = floorPrefab;
            material = floorMaterial;
            buildingName = currentBuildType == BuildType.FloorBelow ? "Floor Below" : "Floor Above";
        }
        
        // Final validation
        if (!IsPlacementValid(buildPosition))
        {
            return;
        }
        
        // Create the actual building
        GameObject newBuilding = Instantiate(prefab, buildPosition, Quaternion.identity);
        newBuilding.name = buildingName;
        
        // Set material if provided
        if (material != null)
        {
            Renderer renderer = newBuilding.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = material;
            
            foreach (Renderer childRenderer in newBuilding.GetComponentsInChildren<Renderer>())
            {
                childRenderer.material = material;
            }
        }
        
        // Enable collider
        foreach (Collider col in newBuilding.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }  
    }
    
    /// <summary>
    /// Set grid size (useful if you want dynamic sizing)
    /// </summary>
    public void SetGridSize(float newGridSize)
    {
        gridSize = newGridSize;
    }
    
    /// <summary>
    /// Get grid size
    /// </summary>
    public float GetGridSize()
    {
        return gridSize;
    }
    
    /// <summary>
    /// Get build distance
    /// </summary>
    public float GetBuildDistance()
    {
        return buildDistance;
    }
    
    /// <summary>
    /// Get build surface layer mask
    /// </summary>
    public LayerMask GetBuildSurface()
    {
        return buildSurface;
    }
}
