using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AutoFitGrid : MonoBehaviour
{
    public int columnCount = 3;
    public float aspectRatio = 1.2f; // height / width

    void Start()
    {
        Fit();
    }

    void Fit()
    {
        var grid = GetComponent<GridLayoutGroup>();
        var rect = GetComponent<RectTransform>();

        float width = rect.rect.width;
        float spacing = grid.spacing.x;
        float padding = grid.padding.left + grid.padding.right;

        float cellWidth = (width - padding - spacing * (columnCount - 1)) / columnCount;
        float cellHeight = cellWidth / aspectRatio;

        grid.cellSize = new Vector2(cellWidth, cellHeight);
    }
}
