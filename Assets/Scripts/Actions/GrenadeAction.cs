using System;
using System.Collections.Generic;
using Fusion;
using Grid;
using MoreMountains.Feedbacks;
using Projectiles;
using Units;
using UnityEngine;

namespace Actions
{
    public class GrenadeAction : BaseAction
    {
        [SerializeField] private int maxThrowDistance = 5;
        [SerializeField] private float grenadeExplosionRadius = 2f;
        [SerializeField] private int grenadeDamageAmount = 30;
        
        [SerializeField] private GrenadeProjectile grenadePrefab;
        [SerializeField] private Transform grenadeSpawnPoint;
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField] private float grenadeFlightDuration = 1.2f;
        
        [Header("Feel Feedbacks")]
        public MMF_Player grenadeFeedbackPlayer;
        
        [Networked] private bool IsThrowing { get; set; }
        [Networked] private float ThrowTimer { get; set; }
        
        private Vector3 _targetWorldPosition;
        private bool _wasThrowing;
        private GrenadeProjectile _spawnedGrenade;
        
        private bool IsLocalPlayerUnit() => _unit.Object.HasInputAuthority;
        public override string GetActionName() => "Grenade";

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            GridPosition unitGridPosition = _unit.GetGridPosition();
            List<GridPosition> validPositions = new List<GridPosition>();
            
            foreach (var pos in ActionUtils.GetGridPositionsInRange(unitGridPosition, maxThrowDistance))
            {
                if (pos != unitGridPosition)
                {
                    validPositions.Add(pos);
                }
            }
            return validPositions;
        }

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete = null)
        {
            if (!Object.HasStateAuthority)
            {
                onActionComplete?.Invoke();
                return;
            }
            var validPositions = GetValidActionGridPositionList();
            if (!ActionUtils.IsValidActionGridPosition(gridPosition, validPositions))
            {
                onActionComplete?.Invoke();
                return;
            }
            StartAction(onActionComplete);
            _targetWorldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);

            if (grenadePrefab != null && grenadeSpawnPoint != null)
            {
                Runner.Spawn(
                    grenadePrefab,
                    grenadeSpawnPoint.position,
                    Quaternion.identity,
                    Object.InputAuthority,
                    (runner, spawnedObj) =>
                    {
                        _spawnedGrenade = spawnedObj.GetComponent<GrenadeProjectile>();
                        if (_spawnedGrenade != null)
                        {
                            Vector3 direction = (_targetWorldPosition - grenadeSpawnPoint.position).normalized;
                            _spawnedGrenade.ThrowGrenade(direction, _targetWorldPosition);
                            _spawnedGrenade.OnGrenadeExplode += HandleGrenadeExplode;
                        }
                    }
                );
            }
            IsThrowing = true;
            ThrowTimer = grenadeFlightDuration;
        }
        
        private void UnsubscribeFromGrenadeEvent()
        {
            if (_spawnedGrenade != null)
            {
                _spawnedGrenade.OnGrenadeExplode -= HandleGrenadeExplode;
                _spawnedGrenade = null;
            }
        }
        
        private void HandleGrenadeExplode(object sender, EventArgs e)
        {
            if (!Object.HasStateAuthority) return;

            GrenadeProjectile grenade = sender as GrenadeProjectile;
            if (grenade == null) return;

            Vector3 explosionPosition = grenade.transform.position;
            ExplodeAtPosition(explosionPosition);
            IsThrowing = false;
            ThrowTimer = 0f;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            if (IsThrowing)
            {
                ThrowTimer -= Runner.DeltaTime;
                if (ThrowTimer <= 0f)
                {
                    IsThrowing = false;
                    ExplodeAtPosition(_targetWorldPosition);
                }
            }
        }

        private void ExplodeAtPosition(Vector3 explosionCenter)
        {
            List<Unit> unitsInRadius = GetUnitsInExplosionRange(explosionCenter, grenadeExplosionRadius);
            float sqrRadius = grenadeExplosionRadius * grenadeExplosionRadius;
            
            foreach (Unit unit in unitsInRadius)
            {
                if (unit == null || !unit.Object || !unit.Object.IsInSimulation) continue;

                Vector3 toUnit = unit.GetWorldPosition() - explosionCenter;
                float sqrDist = toUnit.sqrMagnitude;
                
                if (sqrDist <= sqrRadius)
                {
                    if (!Physics.Raycast(explosionCenter + Vector3.up, toUnit.normalized, toUnit.magnitude, obstacleLayerMask))
                    {
                        unit.Damage(grenadeDamageAmount);
                    }
                }
            }
            UnsubscribeFromGrenadeEvent();
            RPC_PlayGrenadeFeedback();
            ActionComplete();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void RPC_PlayGrenadeFeedback()
        {
            if (grenadeFeedbackPlayer != null)
            {
                grenadeFeedbackPlayer.PlayFeedbacks();
            }
        }

        private List<Unit> GetUnitsInExplosionRange(Vector3 centerWorldPos, float radius)
        {
            GridPosition centerGridPos = LevelGrid.Instance.GetGridPosition(centerWorldPos);
            float cellSize = LevelGrid.Instance.GetCellSize();
            int maxOffset = Mathf.CeilToInt(radius / cellSize);

            List<Unit> result = new List<Unit>();

            foreach (var testPos in ActionUtils.GetGridPositionsInRange(centerGridPos, maxOffset))
            {
                Vector3 cellWorldPos = LevelGrid.Instance.GetWorldPosition(testPos);
                float sqrDist = (cellWorldPos - centerWorldPos).sqrMagnitude;
                if (sqrDist <= (radius * radius))
                {
                    List<Unit> unitsHere = LevelGrid.Instance.GetUnitAtGridPosition(testPos);
                    result.AddRange(unitsHere);
                }
            }
            return result;
        }
        private void OnDestroy()
        {
            UnsubscribeFromGrenadeEvent();
        }
    }
}
