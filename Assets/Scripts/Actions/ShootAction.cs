using System;
using System.Collections.Generic;
using Actions;
using Grid;
using Units;
using UnityEngine;

public class ShootAction : BaseAction
{
    [SerializeField] private int maxShootDistance = 5;
    [SerializeField] private float shootingDuration = .5f;
    [SerializeField] private float cooldownDuration = 1f;
    private Unit currentTargetUnit;
    private Action onShootComplete;
    private float stateTimer;
    
    private enum State
    {
        None,
        Aiming,
        Shooting,
        Cooldown,
    }
    
    private State _state;
    
    public override string GetActionName() => "Shoot";
   
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = _unit.GetGridPosition();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                if ((x * x + z * z) > (maxShootDistance * maxShootDistance)) continue;
                GridPosition offSetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offSetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                if (!LevelGrid.Instance.HasUnitAtGridPosition(testGridPosition)) continue;
                
                List<Unit> targetUnits = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                
                foreach (Unit targetUnit in targetUnits)
                {
                    if (targetUnit.IsEnemy(_unit))
                    {
                        validGridPositionList.Add(testGridPosition);
                        break; // only add the cell once regardless of how many enemies are in the cell.
                    }
                }
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete = null)
    {
        List<Unit> unitsAtPos = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        Unit targetUnit = unitsAtPos.Find(u => u.IsEnemy(_unit));
        
        if (targetUnit == null)
        {
            onActionComplete?.Invoke();
            return;
        }
        
        _unit.SetIsBusy(true);
        
        currentTargetUnit = targetUnit;
        onShootComplete = onActionComplete;
        _state = State.Aiming;
        stateTimer = 2f;
        
        Debug.Log($"{_unit.name} started aiming at {targetUnit.name}");
    }

    private void Update()
    {
        if (_state == State.None) return;
        stateTimer -= Time.deltaTime;
        
        switch (_state)
        {
            case State.Aiming:
                if (Quaternion.Angle(_unit.transform.rotation, Quaternion.LookRotation(currentTargetUnit.transform.position - _unit.transform.position)) < 1f || stateTimer <= 0f)
                {
                    _state = State.Shooting;
                    stateTimer = shootingDuration;
                }
                break;
            case State.Shooting:
                if (stateTimer <= 0f)
                {
                    // Actually fire your bullet, apply damage, trigger muzzle flash, etc.
                    Debug.Log($"{_unit.name} fired at {currentTargetUnit.name}");
                    
                    // Move to cooldown
                    _state = State.Cooldown;
                    stateTimer = cooldownDuration;
                }
                break;
            case State.Cooldown:
                if (stateTimer <= 0f)
                {
                    _state = State.None;
                    _unit.SetIsBusy(false);
                    onShootComplete?.Invoke();
                }
                break;
        }
    }
    
    private void AimAtTarget(Unit targetUnit)
    {
        if (!targetUnit) return;
        
        Vector3 targetDirection = (targetUnit.transform.position - _unit.transform.position).normalized;
        float rotationSpeed = 10f; // Adjust this value for smoothness
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
    
        _unit.transform.rotation = Quaternion.RotateTowards(
            _unit.transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );
    }
    public void SelectTarget(Unit targetUnit)
    {
        currentTargetUnit = targetUnit;
    }
}
