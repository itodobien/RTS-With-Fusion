using Actions;
using Units;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private Unit unit;
    private MoveAction moveAction;
    
    

    /*private void Update()
    {
        // If we don't have a moveAction yet, try to find one
        if (moveAction == null)
        {
            var foundUnit = FindObjectOfType<Unit>();
            if (foundUnit != null)
            {
                unit = foundUnit;
                moveAction = unit.GetMoveAction();
                Debug.Log("Found Unit + MoveAction at runtime!");
            }
        }

        if (Input.GetKeyDown(KeyCode.T) && moveAction != null)
        {
            GridSystemVisual.Instance.HideAllGridPositions();
            GridSystemVisual.Instance.ShowGridPositionList(moveAction.GetValidActionGridPositionList());
        }
    }*/
}
