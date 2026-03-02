using UnityEngine;

/// <summary>
/// Manages visual preview feedback for the building system.
/// Shows where walls and floors will be placed before confirmation.
/// </summary>
public class BuildingPreview : MonoBehaviour
{
    public enum BuildType { Wall, FloorBelow, FloorAbove }
    
    [Header("Settings")]
    [SerializeField] private float gridSize = 2f;
    [SerializeField] private float buildDistance = 10f;
    [SerializeField] private LayerMask buildSurface = -1;
    
    [Header("Materials")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;

    [HideInInspector] public GameObject wallPrefab;
    [HideInInspector] public GameObject floorPrefab;
    
    private GameObject previewWall;
    private GameObject previewFloor;
    private Camera playerCamera;
    private bool showGridPreview = true;
    
    private void Start()
    {
        playerCamera = GetComponentInParent<Camera>();
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
        Vector3 targetPosition = ray.origin + ray.direction * buildDistance;
        
        // Raycast to find exact placement surface
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, buildDistance, buildSurface))
        {
            targetPosition = hit.point;
        }
        
        return targetPosition;
    }
    
    /// <summary>
    /// Calculates the preview position for a floor based on camera position
    /// </summary>
    public Vector3 GetFloorPreviewPosition(BuildType buildType, Vector3 playerPosition)
    {
        Vector3 verticalOffset = Vector3.up * (gridSize / 2);
        
        if (buildType == BuildType.FloorBelow)
            return playerPosition - Vector3.up * gridSize + verticalOffset;
        else
            return playerPosition + Vector3.up * gridSize - verticalOffset;
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
    
    public float GetGridSize()
    {
        return gridSize;
    }
    
    public void SetGridSize(float newGridSize)
    {
        gridSize = newGridSize;
    }
}
