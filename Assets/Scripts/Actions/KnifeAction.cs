using System;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using Grid;
using Managers;
using MoreMountains.Feedbacks;
using Units;
using UnityEngine;

namespace Actions
{
    public class KnifeAction : BaseAction
    {
        public event EventHandler OnStartKnifeAttack;
        public event EventHandler OnStopKnifeAttack;

        [SerializeField] private int knifeAttackRange = 1;
        [SerializeField] private int knifeDamageAmount = 100;
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField] private float shoulderHeight = 1.7f;
        [SerializeField] private int rotationSpeed = 10;
        [SerializeField] private float damageDelay = 0.5f;
        [SerializeField] private float feedbackDelay = 0.5f;
        
        [Header("Feel Feedbacks")]
        public MMF_Player knifeFeedbackPlayer;
    
        [Networked] private bool IsKnifeAttacking { get; set;}
        [Networked] private NetworkId TargetUnitId { get; set; }
        [Networked] private bool IsRotating { get; set; }
        [Networked] private TickTimer DamageTimer { get; set; }
        [Networked] private TickTimer FeedBackTimer { get; set; }
        
        private Unit _targetUnit;
        private Unit GetTargetUnit() => _targetUnit;
        public override string GetActionName() => "Knife";
        public bool GetIsKnifeAttacking() => IsKnifeAttacking;
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayKnifeFeedback()
        {
            if (knifeFeedbackPlayer != null)
            {
                DOVirtual.DelayedCall(feedbackDelay, () =>
                {
                    knifeFeedbackPlayer.PlayFeedbacks();
                    Debug.Log("Playing knife feedback after delay: " + feedbackDelay);
                });
            }
        }

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete = null)
        {
            if (!Object.HasStateAuthority)
            {
                onActionComplete?.Invoke();
                return;
            }

            if (Unit.IsBusy || IsKnifeAttacking)
            {
                onActionComplete?.Invoke();
                return;
            }

            if (!TrySetTargetUnit(gridPosition))
            {
                onActionComplete?.Invoke();
                return;
            }

            StartAction(onActionComplete);
            IsRotating = true;
            IsKnifeAttacking = false;
            FeedBackTimer = TickTimer.CreateFromSeconds(Runner, feedbackDelay); // Start feedback timer here
            OnStartKnifeAttack?.Invoke(this, EventArgs.Empty);
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            if (HandleTargetUnitValidity()) return;

            if (ShouldPlayFeedback())
            {
                RPC_PlayKnifeFeedback();
                FeedBackTimer = TickTimer.None; // Reset the timer after triggering feedback
            }

            if (IsRotating)
            {
                HandleRotation();
            }
            else if (IsKnifeAttacking)
            {
                HandleKnifeAttack();
            }
        }

        private bool HandleTargetUnitValidity()
        {
            if (_targetUnit == null || !_targetUnit.Object || !_targetUnit.Object.IsInSimulation)
            {
                if (TargetUnitId != default)
                {
                    ResetAttackState();
                }
                return true;
            }
            return false;
        }

        private void HandleRotation()
        {
            Vector3 directionToTarget = (_targetUnit.transform.position - transform.position).normalized;
            transform.forward = Vector3.Slerp(transform.forward, directionToTarget, rotationSpeed * Runner.DeltaTime);

            if (Vector3.Dot(transform.forward, directionToTarget) > 0.9f)
            {
                StartKnifeAttack();
            }
        }

        private void HandleKnifeAttack()
        {
            if (ShouldApplyDamage())
            {
                ApplyDamage();
                CompleteKnifeAttack();
            }
            
            if (ShouldPlayFeedback())
            {
                RPC_PlayKnifeFeedback();
                FeedBackTimer = TickTimer.None;
            }
        }

        private bool ShouldApplyDamage()
        {
            return !DamageTimer.Equals(TickTimer.None) && DamageTimer.Expired(Runner);
        }

        private bool ShouldPlayFeedback()
        {
            return !FeedBackTimer.Equals(TickTimer.None) && FeedBackTimer.Expired(Runner);
        }

        private void ApplyDamage()
        {
            _targetUnit.Damage(knifeDamageAmount);
        }

        private void CompleteKnifeAttack()
        {
            ActionComplete();
            ResetAttackState();
        }

        private void StartKnifeAttack()
        {
            IsRotating = false;
            IsKnifeAttacking = true;
            DamageTimer = TickTimer.CreateFromSeconds(Runner, damageDelay);
        }

        private void ResetAttackState()
        {
            IsKnifeAttacking = false;
            IsRotating = false;
            ActionComplete();
            TargetUnitId = default;
            _targetUnit = null;
            OnStopKnifeAttack?.Invoke(this, EventArgs.Empty);
        }

        private bool TrySetTargetUnit(GridPosition gridPosition)
        {
            List<Unit> unitAtPos = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
            if (unitAtPos.Count > 0)
            {
                Unit candidateTarget = unitAtPos[0];

                if (candidateTarget == Unit || candidateTarget.GetTeamID() == Unit.GetTeamID())
                {
                    return false;
                }
                _targetUnit = candidateTarget;
                TargetUnitId = _targetUnit.Object.Id;
                return true;
            }
            return false;
        }

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            GridPosition unitGridPosition = Unit.GetGridPosition();
            List<GridPosition> validGridPositionList = new List<GridPosition>();
            List<GridPosition> enemyPositions = EnemyPositionManager.Instance.GetEnemyPositionsForTeam(Unit.GetTeamID());
    
            foreach (var testPosition in ActionUtils.GetGridPositionsInRange(unitGridPosition, knifeAttackRange))
            {
                if (!enemyPositions.Contains(testPosition)) continue;

                Vector3 attackerPos = Unit.GetWorldPosition() + Vector3.up * shoulderHeight;
                Vector3 targetPos = LevelGrid.Instance.GetWorldPosition(testPosition) + Vector3.up * shoulderHeight;
                Vector3 stabDir = (targetPos - attackerPos).normalized;
                float distanceToTarget = Vector3.Distance(attackerPos, targetPos);

                if (!Physics.Raycast(attackerPos, stabDir, distanceToTarget, obstacleLayerMask))
                {
                    validGridPositionList.Add(testPosition);
                }
            }
            return validGridPositionList;
        }
    }
}
