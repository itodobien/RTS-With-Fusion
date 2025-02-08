using System.Collections.Generic;
using DestructibleObjects;
using Units;
using UnityEngine;

namespace Grid
{
    public class LevelGrid : MonoBehaviour
    {
        public static LevelGrid Instance {get; private set;}
        
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
            _gridSystem = new GridSystem<GridObject>(gridWidth, gridHeight, cellSize, (g, gridPosition) => 
                new GridObject(g, gridPosition));
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
        public List<DestructibleObject> GetObjectsAtGridPosition(GridPosition gridPosition)
        {
            GridObject gridObjectList = _gridSystem.GetGridObject(gridPosition);
            List<DestructibleObject> rawObjectList = gridObjectList.GetObjectList();
            
            List<DestructibleObject> gameObjects = new List<DestructibleObject>();
            foreach (DestructibleObject obj in rawObjectList)
            {
                if (obj != null)
                {
                    gameObjects.Add(obj);
                }
            }
            return gameObjects;
        }

        public Door GetDoorAtGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            return gridObject.GetDoor();
        }

        public void SetDoorAtGridPosition(GridPosition gridPosition, Door door)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            gridObject.SetDoor(door);
        }
        
        public int GetWidth() => _gridSystem.GetWidth();
        public int GetHeight() => _gridSystem.GetHeight();
        public float GetCellSize() => cellSize;
        public GridSystem<GridObject> GetGridSystem() => _gridSystem;
    }
}