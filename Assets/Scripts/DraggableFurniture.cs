using UnityEngine;

public class DraggableFurniture : MonoBehaviour
{
    private bool isDragging;
    private Vector2 offset;
    private SpriteRenderer spriteRenderer;
    private GridManager gridManager;
    public FurnitureData data;
    private Vector3 originalPosition;
    private bool isBeingMoved = false;
    private float doubleClickThreshold = 0.3f;
    private float lastClickTime;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gridManager = FindObjectOfType<GridManager>();
    }

    public void Initialize(FurnitureData furnitureData)
    {
        data = furnitureData;
        spriteRenderer.sprite = data.icon;
    }

    void OnMouseDown()
    {
        // Verificar si el panel principal del inventario esta activo
        if (!InventoryManager.Instance.mainPanel.activeSelf) return;

        originalPosition = transform.position;
        isBeingMoved = true;

        gridManager.MarkAreaOccupied(originalPosition, data.size, gameObject, false);

        offset = (Vector2)transform.position - (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);

       
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            ReturnToInventory();
            return;
        }
        lastClickTime = Time.time;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetPos = gridManager.GetNearestPosition(mousePos + offset);

        // Verificar disponibilidad ignorando este objeto
        bool canPlace = gridManager.IsAreaFree(targetPos, data.size, gameObject);
        spriteRenderer.color = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);

        transform.position = targetPos;
    }

    void OnMouseUp()
    {
        isDragging = false;
        spriteRenderer.color = new Color(1, 1, 1, 1f);

        // Verificar posicion de la grid
        if (gridManager.IsAreaFree(transform.position, data.size, gameObject))
        {
            
            gridManager.MarkAreaOccupied(transform.position, data.size, gameObject, true);
        }
        else
        {
            
            transform.position = originalPosition;
            gridManager.MarkAreaOccupied(originalPosition, data.size, gameObject, true);
        }

        isBeingMoved = false;
    }

    void ReturnToInventory()
    {
        gridManager.MarkAreaOccupied(transform.position, data.size, gameObject, false);
        InventoryManager.Instance.ReturnItem(data); 
        Destroy(gameObject);
    }
}