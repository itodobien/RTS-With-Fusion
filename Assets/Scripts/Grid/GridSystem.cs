using System;
using UnityEngine;

namespace Grid
{
    public class GridSystem<TGridObject>
    {
        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly TGridObject[,] _gridObjectArray;
        
        private Vector2Int _originOffset;
    
        public GridSystem(int width, int height, float cellSize, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            
            _originOffset = new Vector2Int(-width / 2, -height / 2);
            _gridObjectArray = new TGridObject[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    _gridObjectArray[x, z] = createGridObject(this, gridPosition);
                }
            }
        }

        public Vector3 GetWorldPosition(GridPosition gridPosition)
        {
            float worldX = (gridPosition.x + _originOffset.x) * _cellSize;
            float worldZ = (gridPosition.z + _originOffset.y) * _cellSize;
            return new Vector3(worldX, 0, worldZ);
        }

        public GridPosition GetGridPosition(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / _cellSize - _originOffset.x);
            int z = Mathf.RoundToInt(worldPosition.z / _cellSize - _originOffset.y);
            return new GridPosition(x, z);
        }

        /*public void CreateDebugObjects(Transform debugPrefab)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    Vector3 worldPosition = GetWorldPosition(gridPosition);
                    Transform debugTransForm = Object.Instantiate(debugPrefab, worldPosition, Quaternion.identity);
                    GridDebugObject gridDebugObject = debugTransForm.GetComponent<GridDebugObject>();
                    gridDebugObject.SetGridObject(GetGridObject(gridPosition) as GridObject);
                }
            }
        }*/

        public TGridObject GetGridObject(GridPosition gridPosition)
        {
            return _gridObjectArray[gridPosition.x, gridPosition.z];
        }
        
        public bool IsValidGridPosition(GridPosition gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < _width && gridPosition.z >= 0 && gridPosition.z < _height;
        }
        public int GetWidth() => _width;
        public int GetHeight() => _height;
    }
}
