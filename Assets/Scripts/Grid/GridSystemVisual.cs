using System.Collections.Generic;
using Actions;
using Managers;
using Units;
using UnityEngine;

namespace Grid
{
    public class GridSystemVisual : MonoBehaviour
    {
        private static GridSystemVisual Instance {get; set;}
    
        [SerializeField] private Transform gridSystemVisualSinglePrefab;
    
        private GridSystemVisualSingle[,] _gridSystemVisualSingleArray;
        
        private Unit _unit;

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
            int width = LevelGrid.Instance.GetWidth();
            int height = LevelGrid.Instance.GetHeight();
            
            _gridSystemVisualSingleArray = new GridSystemVisualSingle[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    Transform gridSystemVisualSingleTransform = Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
                
                    _gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
                }
            }
        }

        private void Update()
        {
            UpdateGridVisual();
        }

        private void HideAllGridPositions()
        {
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    _gridSystemVisualSingleArray[x, z].Hide();
                }
            }
        }

        private void ShowGridPositionList(List<GridPosition> gridPositionList, BaseAction selectedAction)
        {
            List<GridPosition> enemyPositions = EnemyPositionManager.Instance.GetEnemyPositionsForTeam(_unit.GetTeamID());

            foreach (GridPosition gridPosition in gridPositionList)
            {
                Color gridColor;
                if (selectedAction is InteractAction)
                {
                    gridColor = Color.yellow;
                }
                else
                {
                    bool enemyPresent = enemyPositions.Contains(gridPosition);
                    gridColor = enemyPresent ? Color.red : Color.green;
                }
                _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].SetColor(gridColor);
                _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show();
            }
        }

        private void UpdateGridVisual()
        {
            HideAllGridPositions();
    
            List<Unit> selectedUnits = UI.UnitSelectionManager.Instance.GetSelectedUnits();
            if (selectedUnits.Count == 0) return;
    
            _unit = selectedUnits[0];
    
            BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
            if (selectedAction == null) return;
    
            List<GridPosition> validPosition = selectedAction.GetValidActionGridPositionList();
            ShowGridPositionList(validPosition, selectedAction); // Pass selectedAction here
        }
        
        /*private void UpdateGridVisual()
        {
            HideAllGridPosition();

            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

            GridVisualType gridVisualType;

            switch (selectedAction)
            {
                default:
                case MoveAction moveAction:
                    gridVisualType = GridVisualType.White;
                    break;
                case SpinAction spinAction:
                    gridVisualType = GridVisualType.Blue;
                    break;
                case ShootAction shootAction:
                    gridVisualType = GridVisualType.Red;

                    ShowGridPositionRange(selectedUnit.GetGridPosition(), shootAction.GetMaxShootDistance(), GridVisualType.RedSoft);
                    break;
                case GrenadeAction grenadeAction:
                    gridVisualType = GridVisualType.Yellow;
                    break;
                case SwordAction swordAction:
                    gridVisualType = GridVisualType.Red;

                    ShowGridPositionRangeSquare(selectedUnit.GetGridPosition(), swordAction.GetMaxSwordDistance(), GridVisualType.RedSoft);
                    break;
                case InteractAction interactAction:
                    gridVisualType = GridVisualType.Blue;
                    break;
            }

            ShowGridPositionList(
                selectedAction.GetValidActionGridPositionList(), gridVisualType);
        }*/

    }
}