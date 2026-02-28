using UnityEngine;

[RequireComponent(typeof(Camera))]

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
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float wallThickness = 0.1f;
    
    [Header("Layer Masks")]
    [SerializeField] private LayerMask buildSurface = -1; // What surfaces can walls be placed on
    [SerializeField] private LayerMask buildingLayer; // What layer walls are on
    
    
    [Header("Visual Feedback")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material floorMaterial;
    [SerializeField] private bool showGridPreview = true;
    
    private Camera playerCamera;
    private GameObject previewWall;
    private GameObject previewFloor;
    private bool canPlaceWall = true;
    private BuildType currentBuildType = BuildType.Wall;
    private int buildingResource = 100; // Current resources available
    
    private void Start()
    {
        playerCamera = GetComponent<Camera>();

        playerCamera = Camera.main;
                
        CreatePreviewWall();
        CreatePreviewFloor();
    }
    
    /// <summary>
    /// Determines what type of building to place based on camera direction
    /// </summary>
    private BuildType DetermineBuildType()
    {
        Vector3 forward = playerCamera.transform.forward;
        
        // Check if looking primarily up or down
        if (Mathf.Abs(forward.y) > Mathf.Abs(forward.x) && Mathf.Abs(forward.y) > Mathf.Abs(forward.z))
        {
            if (forward.y > 0)
                return BuildType.FloorAbove;
            else
                return BuildType.FloorBelow;
        }
        
        return BuildType.Wall;
    }
    
    private void Update()
    {
        // Update build type based on camera direction
        currentBuildType = DetermineBuildType();
        
        if (previewWall != null)
        {
            UpdatePreviewPosition();
        }
        
        // Build on left click
        if (Input.GetMouseButton(0) && canPlaceWall)
        {
            PlaceBuilding();
        }
        
        // Toggle preview on Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showGridPreview = !showGridPreview;
            previewWall.SetActive(showGridPreview);
            previewFloor.SetActive(showGridPreview);
        }
    }
    
    /// <summary>
    /// Creates a preview wall to show where the player will build
    /// </summary>
    private void CreatePreviewWall()
    {
        previewWall = Instantiate(wallPrefab);
        previewWall.name = "Preview Wall";
        
        // Disable collider and rigidbody for preview
        foreach (Collider col in previewWall.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        
        Rigidbody rb = previewWall.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        // Set preview material
        SetPreviewMaterial(previewWall, invalidPlacementMaterial);
        
        previewWall.SetActive(showGridPreview);
    }
    
    /// <summary>
    /// Creates a preview floor to show where the player will build
    /// </summary>
    private void CreatePreviewFloor()
    {
        previewFloor = Instantiate(floorPrefab);
        previewFloor.name = "Preview Floor";
        
        // Disable collider and rigidbody for preview
        foreach (Collider col in previewFloor.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        
        Rigidbody rb = previewFloor.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        // Set preview material
        SetPreviewMaterial(previewFloor, invalidPlacementMaterial);
        
        previewFloor.SetActive(false);
    }
    
    // 🚨TECH DEBT TODO🚨: This seems big and like there's a lot of repitition in it
    /// <summary>
    /// Updates the preview position based on camera raycast and build type
    /// </summary>
    private void UpdatePreviewPosition()
    {
        Vector3 snappedPosition = Vector3.zero;
        
        if (currentBuildType == BuildType.Wall)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPosition = ray.origin + ray.direction * buildDistance;
            
            // Raycast to find exact placement surface
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, buildDistance, buildSurface))
            {
                targetPosition = hit.point;
            }
            
            snappedPosition = SnapToGrid(targetPosition);
            
            // Update wall preview
            previewWall.SetActive(true);
            previewFloor.SetActive(false);
            previewWall.transform.position = snappedPosition;
        }
        else if (currentBuildType == BuildType.FloorBelow)
        {
            // Place floor below the player
            snappedPosition = SnapToGrid(transform.position - Vector3.up * gridSize);
            
            previewWall.SetActive(false);
            previewFloor.SetActive(true);
            previewFloor.transform.position = snappedPosition;
        }
        else if (currentBuildType == BuildType.FloorAbove)
        {
            // Place floor above the player
            snappedPosition = SnapToGrid(transform.position + Vector3.up * gridSize);
            
            previewWall.SetActive(false);
            previewFloor.SetActive(true);
            previewFloor.transform.position = snappedPosition;
        }
        
        // Check if placement is valid
        canPlaceWall = IsPlacementValid(snappedPosition);
        
        // Update preview material color based on validity
        Material previewMat = validPlacementMaterial;
        if (!canPlaceWall)
        {
            previewMat = invalidPlacementMaterial;
        }
        
        if (currentBuildType == BuildType.Wall)
            SetPreviewMaterial(previewWall, previewMat);
        else
            SetPreviewMaterial(previewFloor, previewMat);
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
            new Vector3(gridSize / 2f, wallHeight / 2f, gridSize / 2f),
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
        Vector3 buildPosition = Vector3.zero;
        GameObject prefab = null;
        Material material = null;
        string buildingName = "";
        
        if (currentBuildType == BuildType.Wall)
        {
            buildPosition = previewWall.transform.position;
            prefab = wallPrefab;
            material = wallMaterial;
            buildingName = "Wall";
        }
        else if (currentBuildType == BuildType.FloorBelow || currentBuildType == BuildType.FloorAbove)
        {
            buildPosition = previewFloor.transform.position;
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
        
        if (material != null)
        {
            SetPreviewMaterial(newBuilding, material);
        }
        
        // Enable collider
        foreach (Collider col in newBuilding.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }  
    }
    
    /// <summary>
    /// Sets the material for a game object and all its children
    /// </summary>
    private void SetPreviewMaterial(GameObject obj, Material material)
    {
        if (material == null) return;
        
        Renderer renderer = obj.GetComponent<Renderer>();

        renderer.material = material;
        
        foreach (Renderer childRenderer in obj.GetComponentsInChildren<Renderer>())
        {
            childRenderer.material = material;
        }
    }
    
    /// <summary>
    /// Set grid size (useful if you want dynamic sizing)
    /// </summary>
    public void SetGridSize(float newGridSize)
    {
        gridSize = newGridSize;
    }
}
