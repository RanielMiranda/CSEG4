using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    private bool isSelected = false;
    private Renderer objectRenderer;
    private Color originalColor;

    private bool isLevelEditor = false;
    private ObjectMover objectMover;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }

        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            isLevelEditor = true;
        }

        objectMover = FindFirstObjectByType<ObjectMover>(); // Get reference to ObjectMover
    }

    void OnMouseDown()
    {   
        if (isLevelEditor && objectMover != null)
        {
            if (isSelected)
            {
                SelectObject();
            }
            else
            {
                DeselectObject();
            }
        }
    }

    public void SelectObject()
    {
        isSelected = true;
        objectRenderer.material.color = Color.green; // Highlight when selected
        objectMover.SelectObject(gameObject); // Inform ObjectMover
    }

    public void DeselectObject()
    {
        isSelected = false;
        objectRenderer.material.color = originalColor;
        objectMover.DeselectObject(); // Deselect in ObjectMover        
    }
}
