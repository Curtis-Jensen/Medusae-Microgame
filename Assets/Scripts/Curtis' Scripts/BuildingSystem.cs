using UnityEngine;

[RequireComponent(typeof(Camera))]

/// <summary>
/// Fortnite-style grid-based building system for walls only.
/// Attach this to the player and assign a wall prefab.
/// </summary>
public class BuildingSystem : MonoBehaviour
{
    [Header("Building Settings")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float gridSize = 2f; // Size of each grid cell
    [SerializeField] private float buildDistance = 10f; // How far the player can build
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float wallThickness = 0.1f;
    
    [Header("Layer Masks")]
    [SerializeField] private LayerMask buildSurface = -1; // What surfaces can walls be placed on
    [SerializeField] private LayerMask buildingLayer; // What layer walls are on
    
    [Header("Building Cost")]
    [SerializeField] private int wallCost = 10; // Resources needed to build a wall
    
    [Header("Visual Feedback")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private bool showGridPreview = true;
    
    private Camera playerCamera;
    private GameObject previewWall;
    private bool canPlaceWall = true;
    private int buildingResource = 100; // Current resources available
    
    private void Start()
    {
        playerCamera = GetComponent<Camera>();

        playerCamera = Camera.main;
                
        CreatePreviewWall();
    }
    
    private void Update()
    {
        if (previewWall != null)
        {
            UpdatePreviewPosition();
        }
        
        // Build on left click
        if (Input.GetMouseButtonDown(0) && canPlaceWall && buildingResource >= wallCost)
        {
            PlaceWall();
        }
        
        // Toggle preview on Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showGridPreview = !showGridPreview;
            previewWall.SetActive(showGridPreview);
        }
        
        // Debug: Add resources
        if (Input.GetKeyDown(KeyCode.R))
        {
            buildingResource += 100;
            Debug.Log($"Resources: {buildingResource}");
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
    /// Updates the preview wall position based on camera raycast
    /// </summary>
    private void UpdatePreviewPosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = ray.origin + ray.direction * buildDistance;
        
        // Raycast to find exact placement surface
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, buildDistance, buildSurface))
        {
            targetPosition = hit.point;
        }
        
        // Snap to grid
        Vector3 snappedPosition = SnapToGrid(targetPosition);
        
        // Check if placement is valid
        canPlaceWall = IsPlacementValid(snappedPosition);
        
        // Update preview
        previewWall.transform.position = snappedPosition;
        
        // Update preview material color
        Material previewMat = validPlacementMaterial;
        if (!canPlaceWall)
        {
            previewMat = invalidPlacementMaterial;
        }
        
        SetPreviewMaterial(previewWall, previewMat);
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
        if (buildingResource < wallCost)
        {
            return false;
        }
        
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
        
        // Check if player is too close
        float distanceToPlayer = Vector3.Distance(position, transform.position);
        if (distanceToPlayer < gridSize)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Places a wall at the current preview position
    /// </summary>
    private void PlaceWall()
    {
        Vector3 wallPosition = previewWall.transform.position;
        
        // Final validation
        if (!IsPlacementValid(wallPosition))
        {
            return;
        }
        
        // Create the actual wall
        GameObject newWall = Instantiate(wallPrefab, wallPosition, Quaternion.identity);
        newWall.name = "Wall";
        
        // Enable collider
        foreach (Collider col in newWall.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }
        
        // Deduct resources
        buildingResource -= wallCost;
        
        Debug.Log($"Wall placed at {wallPosition}. Resources remaining: {buildingResource}");
    }
    
    /// <summary>
    /// Sets the material for a game object and all its children
    /// </summary>
    private void SetPreviewMaterial(GameObject obj, Material material)
    {
        if (material == null) return;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        
        foreach (Renderer childRenderer in obj.GetComponentsInChildren<Renderer>())
        {
            childRenderer.material = material;
        }
    }
    
    /// <summary>
    /// Add resources to the building system
    /// </summary>
    public void AddResources(int amount)
    {
        buildingResource += amount;
        Debug.Log($"Added {amount} resources. Total: {buildingResource}");
    }
    
    /// <summary>
    /// Get current available resources
    /// </summary>
    public int GetAvailableResources()
    {
        return buildingResource;
    }
    
    /// <summary>
    /// Set grid size (useful if you want dynamic sizing)
    /// </summary>
    public void SetGridSize(float newGridSize)
    {
        gridSize = newGridSize;
    }
}
