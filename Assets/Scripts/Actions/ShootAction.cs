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
        public event EventHandler OnStartShooting;
        public event EventHandler OnStopShooting;
        
        [SerializeField] private int maxShootDistance = 7;
        [SerializeField] private float aimRotationSpeed = 360f;
        [SerializeField] private float aimingTime = 1f;
        [SerializeField] private float firingDuration = 0.3f; 
        [SerializeField] private Projectile bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint; 

        
        [Networked] private float CurrentAimingTime { get; set; }
        [Networked] private bool IsAiming { get; set; }
        [Networked] private bool IsFiring { get; set; }
        [Networked] private float FiringTimer { get; set; }
        private GridPosition targetPosition;
        private Unit _targetUnit;
        
        public bool GetIsFiring() => IsFiring;
        public Unit GetTargetUnit() => _targetUnit;
        
        public override string GetActionName() => "Shoot";
   
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
            CurrentAimingTime = aimingTime;
            IsAiming = true;
            targetPosition = gridPosition;
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
                FiringTimer -= Runner.DeltaTime;
                if (FiringTimer <= 0f)
                {
                    IsFiring = false;
                    ActionComplete();
                }
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

            CurrentAimingTime -= Runner.DeltaTime;
            if (CurrentAimingTime <= 0f && Quaternion.Angle(transform.rotation, targetRotation) < 1f) 
            {
                IsAiming = false;
                HandleFiring();
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
            OnStartShooting?.Invoke(this, EventArgs.Empty);
            Vector3 targetPosition = _targetUnit.GetAimPosition();
            Vector3 shootDirection = (targetPosition - bulletSpawnPoint.position).normalized;
            
            Runner.Spawn(
                bulletPrefab,
                bulletSpawnPoint.position,
                Quaternion.LookRotation(shootDirection),
                Object.InputAuthority,
                (runner, spawnedBullet) =>
                {
                    spawnedBullet.GetComponent<Projectile>().ShootAtTarget(shootDirection);
                }
            );
            
            _targetUnit.Damage();
            Debug.Log($"Unit {_unit.name} fired at position {targetPosition}");
            IsFiring = true;
            FiringTimer = firingDuration;
        }
    }
}