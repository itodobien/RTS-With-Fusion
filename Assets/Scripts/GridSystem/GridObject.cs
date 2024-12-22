using UnityEngine;

public class GridObject

{
    private GridSystem gridSystem;
    private GridPosiiton gridPosition;

    public GridObject(GridSystem gridSystem, GridPosiiton gridPosition)
    {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
    }
    
}
