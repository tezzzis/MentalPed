using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Settings")]
    public Vector2Int gridSize = new Vector2Int(10, 20);
    public float cellSize = 1f;
    public Color gridColor = Color.white;
    public Color occupiedColor = Color.red;

    [Header("Grid Offset")]
    public float gridOffsetX = 0f; 
    public float gridOffsetY = 0f; 

    [Header("Debug")]
    public bool showGrid = true;
    public bool showOccupied = true;

    private GameObject[,] gridOccupied;
    public Dictionary<FurnitureData, GameObject> mueblesColocados = new Dictionary<FurnitureData, GameObject>();

    private void Awake()
    {
        Instance = this;
        InitializeGrid();
    }

    void InitializeGrid()
    {
        gridOccupied = new GameObject[gridSize.x, gridSize.y];
    }

    public Vector2 GetNearestPosition(Vector2 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        return new Vector2(
            gridPos.x * cellSize + cellSize / 2 + gridOffsetX,
            gridPos.y * cellSize + cellSize / 2 + gridOffsetY
        );
    }

    public Vector2Int WorldToGridPosition(Vector2 worldPosition)
    {
        // Aplicar el desplazamiento de la grid
        float adjustedX = worldPosition.x - gridOffsetX;
        float adjustedY = worldPosition.y - gridOffsetY;

        return new Vector2Int(
            Mathf.Clamp(Mathf.FloorToInt(adjustedX / cellSize), 0, gridSize.x - 1),
            Mathf.Clamp(Mathf.FloorToInt(adjustedY / cellSize), 0, gridSize.y - 1)
        );
    }

    public bool IsAreaFree(Vector2 position, Vector2Int size, GameObject ignoreObject = null)
    {
        Vector2Int gridPos = WorldToGridPosition(position);

        // Verificar límites del grid
        if (gridPos.x < 0 || gridPos.y < 0 ||
            gridPos.x + size.x > gridSize.x ||
            gridPos.y + size.y > gridSize.y)
        {
            Debug.Log("Fuera de los límites del grid");
            return false;
        }

        // Verificar celdas ocupadas
        for (int x = gridPos.x; x < gridPos.x + size.x; x++)
        {
            for (int y = gridPos.y; y < gridPos.y + size.y; y++)
            {
                if (gridOccupied[x, y] != null && gridOccupied[x, y] != ignoreObject)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void MarkAreaOccupied(Vector2 position, Vector2Int size, GameObject furniture, bool occupied)
    {
        Vector2Int gridPos = WorldToGridPosition(position);
        FurnitureData data = furniture.GetComponent<DraggableFurniture>().data;

        if (occupied)
        {
            mueblesColocados[data] = furniture;
        }
        else
        {
            mueblesColocados.Remove(data);
        }

        for (int x = gridPos.x; x < gridPos.x + size.x; x++)
        {
            for (int y = gridPos.y; y < gridPos.y + size.y; y++)
            {
                if (x >= 0 && y >= 0 && x < gridSize.x && y < gridSize.y)
                {
                    gridOccupied[x, y] = occupied ? furniture : null;
                }
            }
        }
    }

    public void ResetGrid()
    {
        foreach (var mueble in mueblesColocados.Values)
        {
            Destroy(mueble);
        }
        mueblesColocados.Clear();
        InitializeGrid();
    }

    void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = gridColor;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 center = new Vector3(
                    x * cellSize + cellSize / 2 + gridOffsetX,
                    y * cellSize + cellSize / 2 + gridOffsetY,
                    0
                );

                Gizmos.DrawWireCube(center, Vector3.one * cellSize);

                if (showOccupied && Application.isPlaying && gridOccupied != null)
                {
                    if (gridOccupied[x, y] != null)
                    {
                        Gizmos.color = occupiedColor;
                        Gizmos.DrawCube(center, Vector3.one * cellSize * 0.9f);
                        Gizmos.color = gridColor;
                    }
                }
            }
        }
    }
}