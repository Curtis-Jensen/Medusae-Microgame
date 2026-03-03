using UnityEngine;

/// <summary>
/// Manages visual preview feedback for the building system.
/// Shows where walls and floors will be placed before confirmation.
/// </summary>
public class BuildingPreview : MonoBehaviour
{
    public enum BuildType { Wall, FloorBelow, FloorAbove }
    
    [Header("Materials")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;

    [HideInInspector] public GameObject wallPrefab;
    [HideInInspector] public GameObject floorPrefab;
    
    private GameObject previewWall;
    private GameObject previewFloor;
    private Camera playerCamera;
    private ConstructionSystem ConstructionSystem;
    private bool showGridPreview = true;
    
    private void Start()
    {
        playerCamera = GetComponentInParent<Camera>();
        ConstructionSystem = GetComponent<ConstructionSystem>();
        CreatePreviewWall();
        CreatePreviewFloor();
    }
    
    private void Update()
    {
        // Toggle preview on Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePreviewVisibility();
        }
    }
    
    /// <summary>
    /// Updates the preview position and material based on build type and validity
    /// </summary>
    public void UpdatePreview(BuildType buildType, Vector3 snappedPosition, bool isValid)
    {
        bool isWall = buildType == BuildType.Wall;
        previewWall.SetActive(isWall && showGridPreview);
        previewFloor.SetActive(!isWall && showGridPreview);
        
        // Update position
        if (isWall)
            previewWall.transform.position = snappedPosition;
        else
            previewFloor.transform.position = snappedPosition;
        
        // Update material based on validity
        Material previewMat = isValid ? validPlacementMaterial : invalidPlacementMaterial;
        GameObject previewObj = isWall ? previewWall : previewFloor;
        SetPreviewMaterial(previewObj, previewMat);
    }
    
    /// <summary>
    /// Calculates the preview position for a wall based on raycast
    /// </summary>
    public Vector3 GetWallPreviewPosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = ray.origin + ray.direction * ConstructionSystem.GetBuildDistance();
        
        // Raycast to find exact placement surface
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, ConstructionSystem.GetBuildDistance(), ConstructionSystem.GetBuildSurface()))
        {
            targetPosition = hit.point;
        }
        
        return targetPosition;
    }
    
    /// <summary>
    /// Calculates the preview position for a floor based on raycasting downward/upward
    /// </summary>
    public Vector3 GetFloorPreviewPosition(BuildType buildType, Vector3 playerPosition)
    {
        float gridSize = ConstructionSystem.GetGridSize();
        Vector3 rayDirection = buildType == BuildType.FloorBelow ? Vector3.down : Vector3.up;
        float rayDistance = 100f; // Search far up/down for a surface
        
        Vector3 targetPosition = playerPosition;
        
        // Raycast to find the nearest surface below/above
        RaycastHit hit;
        if (Physics.Raycast(playerPosition, rayDirection, out hit, rayDistance))
        {
            targetPosition = hit.point;
            
            // Place the floor just above/below the hit surface
            if (buildType == BuildType.FloorBelow)
                targetPosition = hit.point + Vector3.down * (gridSize / 2);
            else
                targetPosition = hit.point + Vector3.up * (gridSize / 2);
        }
        else
        {
            // Fallback to offset position if no surface found
            if (buildType == BuildType.FloorBelow)
                targetPosition = playerPosition - Vector3.up * gridSize;
            else
                targetPosition = playerPosition + Vector3.up * gridSize;
        }
        
        return targetPosition;
    }
    
    /// <summary>
    /// Toggles preview visibility
    /// </summary>
    public void TogglePreviewVisibility()
    {
        showGridPreview = !showGridPreview;
        previewWall.SetActive(showGridPreview && previewWall.activeSelf);
        previewFloor.SetActive(showGridPreview && previewFloor.activeSelf);
    }
    
    /// <summary>
    /// Gets the snapped position for the current build type
    /// </summary>
    public Vector3 GetPreviewPosition(BuildType buildType)
    {
        Vector3 previewPosition;
        
        if (buildType == BuildType.Wall)
        {
            previewPosition = GetWallPreviewPosition();
        }
        else
        {
            previewPosition = GetFloorPreviewPosition(buildType, ConstructionSystem.transform.position);
        }
        
        return SnapToGrid(previewPosition);
    }
    
    /// <summary>
    /// Snaps a position to the grid
    /// </summary>
    private Vector3 SnapToGrid(Vector3 position)
    {
        float gridSize = ConstructionSystem.GetGridSize();
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.y = Mathf.Round(position.y / gridSize) * gridSize;
        position.z = Mathf.Round(position.z / gridSize) * gridSize;
        return position;
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
        
        previewWall.SetActive(false);
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
    
    /// <summary>
    /// Sets the material for a game object and all its children
    /// </summary>
    private void SetPreviewMaterial(GameObject obj, Material material)
    {
        if (material == null) return;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material = material;
        
        foreach (Renderer childRenderer in obj.GetComponentsInChildren<Renderer>())
        {
            childRenderer.material = material;
        }
    }
    

}
