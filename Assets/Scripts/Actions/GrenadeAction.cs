using System;
using System.Collections.Generic;
using System.Linq;
using DestructibleObjects;
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
        public event EventHandler OnGrenadeAmountChanged;
        [SerializeField] private int maxThrowDistance = 5;
        [SerializeField] private float grenadeExplosionRadius = 4f;
        [SerializeField] private int grenadeDamageAmount = 60;

        [SerializeField] private GrenadeProjectile grenadePrefab;
        [SerializeField] private Transform grenadeSpawnPoint;
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField] private float grenadeFlightDuration = 1.2f;
        

        [Header("Feel Feedbacks")] public MMF_Player grenadeFeedbackPlayer;

        [Networked] private bool IsThrowing { get; set; }
        [Networked] private float ThrowTimer { get; set; }
        [Networked] private TickTimer life { get; set; }
        [Networked] private int grenadeAmount { get; set; } = 4;
        

        private Vector3 _targetWorldPosition;
        private bool _wasThrowing;
        private GrenadeProjectile _spawnedGrenade;
        private int grenadeDelayTime = 2;

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

            if (grenadePrefab != null && grenadeSpawnPoint != null && grenadeAmount > 0 && life.ExpiredOrNotRunning(Runner))
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
                            RPC_UpdateGrenadeAmount(grenadeAmount - 1);

                            life = TickTimer.CreateFromSeconds(Runner, grenadeDelayTime); 
                            Debug.Log($"[Grenade] Grenade amount: {grenadeAmount}");
                            if (grenadeAmount <= 0)
                            {
                                ActionComplete();
                            }
                        }
                    }
                );
            }
            IsThrowing = true;
            ThrowTimer = grenadeFlightDuration;
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateGrenadeAmount(int newAmount)
        {
            grenadeAmount = newAmount;
            OnGrenadeAmountChanged?.Invoke(this, EventArgs.Empty);
        }

        public int GetGrenadeAmount() => grenadeAmount;

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
            if (!IsThrowing) return;

            GrenadeProjectile grenade = sender as GrenadeProjectile;
            if (grenade == null) return;

            Vector3 explosionPosition = grenade.transform.position;
            IsThrowing = false;
            ThrowTimer = 0f;
            RPC_PlayGrenadeFeedback();

            ExplodeAtPosition(explosionPosition);
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
                }
            }
        }

        private void ExplodeAtPosition(Vector3 explosionCenter)
        {
            // Units
            var unitsInRadius = GetUnitsInExplosionRange(explosionCenter, grenadeExplosionRadius);
            ApplyExplosionDamage(unitsInRadius, explosionCenter, grenadeExplosionRadius, obstacleLayerMask);

            // Destructible Objects
            var objectsInRange = GetObjectsInExplosionRange(explosionCenter, grenadeExplosionRadius);
            ApplyExplosionDamage(objectsInRange, explosionCenter, grenadeExplosionRadius, obstacleLayerMask);

            // Recalculate paths for all units
            var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None).ToList();
            foreach (var unit in allUnits)
            {
                if (unit?.Object?.IsInSimulation == true)
                    unit.ForceRecalculatePath();
            }
    
            UnsubscribeFromGrenadeEvent();
            ActionComplete();
        }
        
        private void UpdateGrenadeAmount(int newAmount)
        {
            if (grenadeAmount != newAmount)
            {
                grenadeAmount = newAmount;
                OnGrenadeAmountChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        private Vector3 GetPosition(object target)
        {
            switch (target)
            {
                case Unit u:
                    return u.GetWorldPosition();
                case DestructibleObject d:
                    return d.transform.position;
                default:
                    return Vector3.zero;
            }
        }

        private void DamageTarget(object target)
        {
            switch (target)
            {
                case Unit u:
                    u.Damage(grenadeDamageAmount);
                    break;
                case DestructibleObject d:
                    d.Damage(grenadeDamageAmount);
                    break;
            }
        }
        
        private List<T> GetInExplosionRange<T>(
            Vector3 centerWorldPos,
            float radius,
            Func<GridPosition, List<T>> getEntitiesAtGridPos)
        {
            var centerGridPos = LevelGrid.Instance.GetGridPosition(centerWorldPos);
            float cellSize = LevelGrid.Instance.GetCellSize();
            int maxOffset = Mathf.CeilToInt(radius / cellSize);
            float sqrRadius = radius * radius;

            List<T> result = new List<T>();
            foreach (var testPos in ActionUtils.GetGridPositionsInRange(centerGridPos, maxOffset))
            {
                Vector3 cellWorldPos = LevelGrid.Instance.GetWorldPosition(testPos);
                if ((cellWorldPos - centerWorldPos).sqrMagnitude <= sqrRadius)
                    result.AddRange(getEntitiesAtGridPos(testPos));
            }
            return result;
        }
        private void ApplyExplosionDamage<T>(IEnumerable<T> targets, Vector3 center, float radius, LayerMask obstacle)
        {
            float sqrRadius = radius * radius;
            foreach (var target in targets)
            {
                Vector3 direction = GetPosition(target) - center;
                if (direction.sqrMagnitude > sqrRadius) continue;
                if (Physics.Raycast(center + Vector3.up, direction.normalized, direction.magnitude, obstacle)) continue;
                DamageTarget(target);
            }
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayGrenadeFeedback()
        {
            Debug.Log($"[Grenade] Feedback RPC called on client: {Runner.LocalPlayer}");
            if (grenadeFeedbackPlayer != null)
            {
                grenadeFeedbackPlayer.PlayFeedbacks();
            }
        }

        private List<Unit> GetUnitsInExplosionRange(Vector3 centerWorldPos, float radius)
        {
            return GetInExplosionRange(
                centerWorldPos,
                radius,
                pos => LevelGrid.Instance.GetUnitAtGridPosition(pos)
            );
        }
        
        private List<DestructibleObject> GetObjectsInExplosionRange(Vector3 centerWorldPos, float radius)
        {
            return GetInExplosionRange(
                centerWorldPos,
                radius,
                pos => LevelGrid.Instance.GetObjectsAtGridPosition(pos)
            );
        }

        private void OnDestroy()
        {
            UnsubscribeFromGrenadeEvent();
        }
    }
}