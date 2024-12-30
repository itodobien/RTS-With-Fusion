using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Grid
{
    public class LevelGrid : MonoBehaviour
    {
        public static LevelGrid Instance {get; private set;}
    
    
        [SerializeField] private Transform gridDebugObjectPrefab;
    
        private GridSystem _gridSystem;
        private void Awake()
        {
        
            if (Instance != null)
            {
                Debug.LogError("More than one LevelGrid in scene");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _gridSystem = new GridSystem(10, 10, 2f);
            _gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
        
        }

        public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            gridObject.AddUnit(unit);
        }

        

        private void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
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

        public bool HasUnitAtGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            return gridObject.HasAnyUnit();
        }
        public List<Unit> GetUnitAtGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            return gridObject.GetUnitList();
        }
    
    }
}
