using System;
using System.Collections.Generic;
using Fusion;
using Grid;
using Managers;
using MoreMountains.Feedbacks;
using Projectiles;
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
        [SerializeField] private AutomaticRifleProjectile bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private int shotDamageAmount = 40;
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField] private float shoulderHeight = 1.7f;
        
        [Header("Feel Feedbacks")]
        public MMF_Player fireFeedbackPlayer;
        
        [Networked] private float CurrentAimingTime { get; set; }
        [Networked] private bool IsAiming { get; set; }
        [Networked] private bool IsFiring { get; set; }
        [Networked] private float FiringTimer { get; set; }
        [Networked] private NetworkId TargetUnitId { get; set; }
        private GridPosition _targetPosition;
        private Unit _targetUnit;
        
        public bool GetIsFiring() => IsFiring;
        private bool _wasFiring;
        public Unit GetTargetUnit() => _targetUnit;
        
        public override string GetActionName() => "Shoot";
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayBlasterFeedback()
        {
            if (fireFeedbackPlayer != null)
            {
                fireFeedbackPlayer.PlayFeedbacks();
            }
        }
   
        public override List<GridPosition> GetValidActionGridPositionList()
        {
            GridPosition unitGridPosition = Unit.GetGridPosition();
            List<GridPosition> validGridPositionList = new List<GridPosition>();

            foreach (var enemyPosition in EnemyPositionManager.Instance.GetEnemyPositionsForTeam(Unit.GetTeamID()))
            {
                int distance = GridPosition.GetDistance(unitGridPosition, enemyPosition);
                if (distance <= maxShootDistance)
                {
                    Vector3 shooterPos = Unit.GetWorldPosition() + Vector3.up * shoulderHeight;
                    Vector3 targetPos = LevelGrid.Instance.GetWorldPosition(enemyPosition) + Vector3.up * shoulderHeight;
                    Vector3 shootDir = (targetPos - shooterPos).normalized;
                    float distanceToTarget = Vector3.Distance(shooterPos, targetPos);

                    if (!Physics.Raycast(shooterPos, shootDir, distanceToTarget, obstacleLayerMask))
                    {
                        validGridPositionList.Add(enemyPosition);
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
            if (Unit.IsBusy || IsFiring || IsAiming)
            {
                onActionComplete?.Invoke();
                return;
            }
            
            List<Unit> unitAtPos = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
            if (unitAtPos.Count > 0)
            {
                Unit candidateTarget = unitAtPos[0];

                if (candidateTarget == Unit || candidateTarget.GetTeamID() == Unit.GetTeamID())
                {
                    onActionComplete?.Invoke();
                    return;
                }
                _targetUnit = candidateTarget;
                TargetUnitId = _targetUnit.Object.Id;
            }
            else
            {
                onActionComplete?.Invoke();
                return;
            }

            StartAction(onActionComplete);
            CurrentAimingTime = aimingTime;
            IsAiming = true;
            _targetPosition = gridPosition;
        }
        
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            
            if (_targetUnit == null || !_targetUnit.Object || !_targetUnit.Object.IsInSimulation)
            {
                if (TargetUnitId != default)
                {
                    IsFiring = false;
                    IsAiming = false;
                    ActionComplete();
                }
                return;
            }

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
                    OnStopShooting?.Invoke(this, EventArgs.Empty);
                    ActionComplete();
                }
            }
        }
        private void HandleAiming()
        {
            if (_targetUnit == null)
            {
                IsAiming = false;
                ActionComplete();
                return;
            }
            Vector3 direction = (_targetUnit.GetWorldPosition() - Unit.GetWorldPosition()).normalized;
            direction.y = 0; 
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, aimRotationSpeed * Runner.DeltaTime);

            CurrentAimingTime -= Runner.DeltaTime;
            if (CurrentAimingTime <= 0f && Quaternion.Angle(transform.rotation, targetRotation) < 1f) 
            {
                IsAiming = false;
                HandleFiring();
            }
        }
        private void HandleFiring()
        {
            if (_targetUnit == null || !_targetUnit.Object || !_targetUnit.Object.IsInSimulation)
            {
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
                    spawnedBullet.GetComponent<AutomaticRifleProjectile>().ShootAtTarget(shootDirection, targetPosition);
                }
            );
            
            _targetUnit.Damage(shotDamageAmount);
            IsFiring = true;
            FiringTimer = firingDuration;
            RPC_PlayBlasterFeedback();
        }
    }
}