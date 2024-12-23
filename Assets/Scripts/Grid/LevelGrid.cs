using System.Collections.Generic;
using Unit_Activities;
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

        public List<Unit> GetUnitAtGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            return gridObject.GetUnitList();
        }

        public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
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
    
    }
}
