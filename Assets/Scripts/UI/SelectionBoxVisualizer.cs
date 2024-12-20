/*using UnityEngine;

public class SelectionBoxVisualizer : MonoBehaviour
{
    [SerializeField] private RectTransform selectionBoxVisual;

    private Vector2 selectionBoxStart;
    private Vector2 selectionBoxEnd;

    private bool isSelecting;
    public bool IsSelecting => isSelecting;

    public Rect SelectionRect { get; private set; }

    private void Start()
    {
        selectionBoxVisual.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandleMouseInput();
        if (isSelecting)
        {
            UpdateSelectionBox();
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndSelection();
        }
    }

    private void StartSelection()
    {
        isSelecting = true;
        selectionBoxStart = Input.mousePosition;
        selectionBoxVisual.gameObject.SetActive(true);
    }

    private void EndSelection()
    {
        isSelecting = false;
        selectionBoxVisual.gameObject.SetActive(false);
    }

    private void UpdateSelectionBox()
    {
        selectionBoxEnd = Input.mousePosition;

        Vector2 boxStart = selectionBoxStart;
        Vector2 boxEnd = selectionBoxEnd;

        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        selectionBoxVisual.position = boxCenter;

        Vector2 boxSize = new Vector2(
            Mathf.Abs(boxEnd.x - boxStart.x),
            Mathf.Abs(boxEnd.y - boxStart.y)
        );
        selectionBoxVisual.sizeDelta = boxSize;

        SelectionRect = GetScreenRect(selectionBoxStart, selectionBoxEnd);
    }

    private Rect GetScreenRect(Vector2 start, Vector2 end)
    {
        start.y = Screen.height - start.y; 
        end.y = Screen.height - end.y;
        Vector2 bottomLeft = Vector2.Min(start, end);
        Vector2 topRight = Vector2.Max(start, end);
        return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
    }
}*/