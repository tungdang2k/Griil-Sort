using UnityEngine;
using UnityEngine.UI;

public class ResponsiveGridLayout : MonoBehaviour
{
    public GridLayoutGroup grid;
    public int column = 3;
    public float spacing = 0;

    void Start()
    {
        UpdateGrid();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateGrid();
    }

    void UpdateGrid()
    {
        if (grid == null) return;

        RectTransform rt = grid.GetComponent<RectTransform>();

        float width = rt.rect.width;
        float height = rt.rect.height;

        int column = 3;
        int row = Mathf.CeilToInt(grid.transform.childCount / (float)column);

        float spacingX = grid.spacing.x;
        float spacingY = grid.spacing.y;

        float cellWidth = (width - (column - 1) * spacingX) / column;

        float cellHeight = (height - (row - 1) * spacingY) / row;

        float size = Mathf.Min(cellWidth, cellHeight);

        grid.cellSize = new Vector2(size, size);
    }
}