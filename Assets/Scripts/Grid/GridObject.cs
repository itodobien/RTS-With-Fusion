using System.Collections.Generic;
using Units;

namespace Grid
{
    public class GridObject
    {
        private GridSystem<GridObject> _gridSystem;
        private readonly List<Unit> _unitList;
        private readonly GridPosition _gridPosition;

        public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
        {
            _gridPosition = gridPosition;
            _unitList = new List<Unit>();
        }

        public override string ToString()
        {
            string unitString = "";
            foreach (Unit unit in _unitList)
            {
                unitString += unit + "\n";
            }
            return _gridPosition + "\n" + unitString;
        }

        public void AddUnit(Unit unit)
        {
            _unitList.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            _unitList.Remove(unit);
        }

        public List<Unit> GetUnitList()
        {
            return _unitList;
        }

        public bool HasAnyUnit()
        {
            return _unitList.Count > 0;
        }

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