using Unit_Activities;
using UnityEngine;

public class Testing : MonoBehaviour
{
    
    private GridSystem gridSystem;
    [SerializeField] private Transform gridDebugObjectPrefab;
    
    void Start()
    {
        gridSystem = new GridSystem(20, 20, 2f);
        gridSystem.CreateDebegObjects(gridDebugObjectPrefab);
        Debug.Log(new GridPosiiton(5, 7));
    }

    private void Update()
    {
        
        Debug.Log(gridSystem.GetGridPosition(MouseWorldPosition.GetMouseWorldPosition()));
    }
}
