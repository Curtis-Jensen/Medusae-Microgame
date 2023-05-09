using UnityEngine;

public class SpawnPointGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.green;
    public float gizmoSize = 0.5f;

    private void OnMouseDown()
    {
        // Select the game object in the Scene view
        UnityEditor.Selection.activeGameObject = gameObject;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);
    }
}
