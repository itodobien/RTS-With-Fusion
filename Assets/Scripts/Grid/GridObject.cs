using System.Collections.Generic;
using DestructibleObjects;
using Units;

namespace Grid
{
    public class GridObject
    {
        private GridSystem<GridObject> _gridSystem;
        private readonly List<Unit> _unitList;
        private readonly List<DestructibleObject> _objectList;
        private readonly GridPosition _gridPosition;

        public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
        {
            _gridPosition = gridPosition;
            _gridSystem = gridSystem;
            _unitList = new List<Unit>();
            _objectList = new List<DestructibleObject>();
        }

        public override string ToString()
        {
            string unitString = "";
            foreach (Unit unit in _unitList)
            {
                unitString += unit + "\n";
            }
            return $"x: {_gridPosition.x / 2}, z: {_gridPosition.z / 2}\n" + unitString;
        }

        public void AddUnit(Unit unit) => _unitList.Add(unit);
        public void RemoveUnit(Unit unit) => _unitList.Remove(unit);
        public List<Unit> GetUnitList() => _unitList;
        public bool HasAnyUnit() => _unitList.Count > 0;
       
        public void AddDestructibleObject (DestructibleObject destructibleObject) => _objectList.Add(destructibleObject);
        public void RemoveDestructibleObject(DestructibleObject destructibleObject) => _objectList.Remove(destructibleObject);

        public List<DestructibleObject> GetObjectList() => _objectList;
        public bool HasAnyObject() => _objectList.Count > 0;

        public Unit GetUnit()
        {
            if (HasAnyUnit())
            {
                return _unitList[0];
            }
            return null;
        }
    }
}