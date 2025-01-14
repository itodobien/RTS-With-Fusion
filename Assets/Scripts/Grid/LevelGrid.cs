using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Grid
{
    public class LevelGrid : MonoBehaviour
    {
        public static LevelGrid Instance {get; private set;}
        
        [SerializeField] private Transform gridDebugObjectPrefab;
        [SerializeField] private int gridWidth = 20;
        [SerializeField] private int gridHeight = 20;
        [SerializeField] private float cellSize = 2f;
        private GridSystem<GridObject> _gridSystem;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("More than one LevelGrid in scene");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _gridSystem = new GridSystem<GridObject>(gridWidth, gridHeight, cellSize, (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
            _gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
        }

        public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            gridObject.AddUnit(unit);
        }

        internal void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            gridObject.RemoveUnit(unit);
        }

        public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
        {
            RemoveUnitAtGridPosition(fromGridPosition, unit);
            AddUnitAtGridPosition(toGridPosition, unit);
        }

        public GridPosition GetGridPosition(Vector3 worldPosition) => _gridSystem.GetGridPosition(worldPosition);
        public Vector3 GetWorldPosition(GridPosition gridPosition) => _gridSystem.GetWorldPosition(gridPosition);
        public bool IsValidGridPosition(GridPosition gridPosition) => _gridSystem.IsValidGridPosition(gridPosition);
        
        public int GetWidth() => _gridSystem.GetWidth();
        public int GetHeight() => _gridSystem.GetHeight();
        public GridSystem<GridObject> GetGridSystem() => _gridSystem;


        public bool HasUnitAtGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);

            foreach (Unit unit in gridObject.GetUnitList())
            {
                if (unit != null && unit.Object && unit.Object.IsInSimulation)
                {
                    return true;
                }
            }
            return false;
        }
        public List<Unit> GetUnitAtGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            List<Unit> rawList = gridObject.GetUnitList();
            
            List<Unit> validUnits = new List<Unit>();
            foreach (Unit unit in rawList)
            {
                if (unit != null && unit.Object && unit.Object.IsInSimulation)
                {
                    validUnits.Add(unit);
                }
            }
            return validUnits;
        }
    }
}