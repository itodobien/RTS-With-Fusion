using System.Collections.Generic;
using Grid;

namespace Actions
{
    public static class ActionUtils
    {
        public static IEnumerable<GridPosition> GetGridPositionsInRange(GridPosition center, int range)
        {
            for (int x = -range; x <= range; x++)
            {
                for (int z = -range; z <= range; z++)
                {
                    GridPosition testPosition = center + new GridPosition(x, z);
                    if (LevelGrid.Instance.IsValidGridPosition(testPosition))
                    {
                        yield return testPosition;
                    }
                }
            }
        }

        public static bool IsValidActionGridPosition(GridPosition gridPosition, List<GridPosition> validPositions)
        {
            return validPositions.Contains(gridPosition);
        }
    }
}