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

        private void ShowGridPositionList(List<GridPosition> gridPositionList)
        {
            List<GridPosition> enemyPositions = EnemyPositionManager.Instance.GetEnemyPositions();

            foreach (GridPosition gridPosition in gridPositionList)
            {
                bool enemyPresent = enemyPositions.Contains(gridPosition);
                Color gridColor = enemyPresent ? Color.red : Color.green;
                _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].SetColor(gridColor);
                _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show();
            }
        }



        private void UpdateGridVisual()
        {
            HideAllGridPositions();
        
            List<Unit> selectedUnits = UI.UnitSelectionManager.Instance.GetSelectedUnits();
            if (selectedUnits.Count == 0) return;
            
            BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
            if (selectedAction == null) return;
            
            List<GridPosition> validPosition = selectedAction.GetValidActionGridPositionList();
            ShowGridPositionList(validPosition);
        }
    }
}