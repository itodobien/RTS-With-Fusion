using System.Collections.Generic;
using Grid;
using Units;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{
    public static GridSystemVisual Instance {get; private set;}
    
    [SerializeField] private Transform gridSystemVisualSinglePrefab;
    
    private GridSystemVisualSingle[,] _gridSystemVisualSingleArray;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GridSystemVisual in scene");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _gridSystemVisualSingleArray = new GridSystemVisualSingle[LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetHeight()];
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Transform gridSystemVisualSingleTransform = Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPostion(gridPosition), Quaternion.identity);
                
                _gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
            }
        }
    }

    private void Update()
    {
        UpdateGridVisual();
    }
    
    public void HideAllGridPositions()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                _gridSystemVisualSingleArray[x, z].Hide();
            }
        }
    }

    public void ShowGridPositionList(List<GridPosition> gridPositionList)
    {
        foreach (GridPosition gridPosition in gridPositionList)
        {
            _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show();
        }
        
    }

    private void UpdateGridVisual()
    {
        HideAllGridPositions();
        
        List<Unit> selectedUnits = UI.UnitSelectionManager.Instance.GetSelectedUnits();

        foreach (var unit in selectedUnits)
        {
            var moveAction = unit.GetMoveAction();
            if (moveAction != null)
            {
                List<GridPosition> validMoves = moveAction.GetValidActionGridPositionList();
                ShowGridPositionList(validMoves);
            }
        }
    }
}
