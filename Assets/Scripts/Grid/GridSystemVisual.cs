using Grid;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{
    [SerializeField] private Transform gridSystemVisualSinglePrefab;

    private void Start()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPostion(gridPosition), Quaternion.identity);
            }
        }
    }
    
}
