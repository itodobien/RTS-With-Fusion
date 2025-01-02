using System;
using System.Collections.Generic;
using Fusion;
using Grid;
using Units;
using UnityEngine;

namespace Actions
{
    public class ShootAction : BaseAction
    {
        [SerializeField] private int maxShootDistance = 7;
        [SerializeField] private float aimRotationSpeed = 360f;
        [SerializeField] private float aimingTime = 1f;
        
        [Networked] private float currentAimingTime { get; set; }
        [Networked] private bool IsAiming { get; set; }
        [Networked] private bool IsFiring { get; set; }

        private bool canShootBullet;
        
        private GridPosition targetPosition;
        private Unit _targetUnit;
        
        public override string GetActionName() => "Shoot";

        protected override void Awake()
        {
            base.Awake();
        }
   
        public override List<GridPosition> GetValidActionGridPositionList()
        {
            List<GridPosition> validGridPositionList = new List<GridPosition>();
            GridPosition unitGridPosition = _unit.GetGridPosition();

            for (int x = -maxShootDistance; x <= maxShootDistance; x++)
            {
                for (int z = -maxShootDistance; z <= maxShootDistance; z++)
                {
                    GridPosition testPosition = unitGridPosition + new GridPosition(x, z);

                    if (!LevelGrid.Instance.IsValidGridPosition(testPosition)) continue;
                    if (!LevelGrid.Instance.HasUnitAtGridPosition(testPosition)) continue;

                    foreach (Unit potentialTarget in LevelGrid.Instance.GetUnitAtGridPosition(testPosition))
                    {
                        if (potentialTarget.GetTeamID() != _unit.GetTeamID())
                        {
                            validGridPositionList.Add(testPosition);
                            break;
                        }
                    }
                }
            }
            return validGridPositionList;
        }

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete = null)
        {
            
            if (!Object.HasStateAuthority)
            {
                onActionComplete?.Invoke();
                return;
            }
            if (_unit.IsBusy || IsFiring || IsAiming)
            {
                onActionComplete?.Invoke();
                return;
            }
            
            List<Unit> unitAtPos = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
            if (unitAtPos.Count > 0)
            {
                _targetUnit = unitAtPos[0];
            }
            else
            {
                onActionComplete?.Invoke();
                return;
            }
            
            StartAction(onActionComplete);
            currentAimingTime = aimingTime;
            IsAiming = true;
            targetPosition = gridPosition;
            canShootBullet = true;
        }
        
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            if (IsAiming)
            {
                HandleAiming();
            }
            else if (IsFiring)
            {
                HandleFiring();
            }
        }
        private void HandleAiming()
        {
            if (_targetUnit == null)
            {
                Debug.LogWarning("Tried to ain with no targets");
                IsAiming = false;
                ActionComplete();
                return;
            }
            Vector3 direction = (_targetUnit.GetWorldPosition() - _unit.GetWorldPosition()).normalized;
            direction.y = 0; // Only rotate on the Y-axis for 2D plane.
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, aimRotationSpeed * Runner.DeltaTime);

            currentAimingTime -= Runner.DeltaTime;
            if (currentAimingTime <= 0f && Quaternion.Angle(transform.rotation, targetRotation) < 1f) 
            {
                IsAiming = false;
                IsFiring = true;
                Debug.Log("Aiming complete. Transitioning to firing...");
            }
        }
        private void HandleFiring()
        {
            if (_targetUnit == null)
            {
                Debug.LogWarning("Tried to fire with no targets");
                IsFiring = false;
                ActionComplete();
                return;
            }
            _targetUnit.Damage();
            Debug.Log($"Unit {_unit.name} fired at position {targetPosition}");
            IsFiring = false;

            ActionComplete();
        }
        
    }
}