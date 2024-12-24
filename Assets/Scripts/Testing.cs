using Units;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private Unit unit;
    
    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            unit.GetMoveAction().GetValidActionGridPositionList();
        }
    }
}
