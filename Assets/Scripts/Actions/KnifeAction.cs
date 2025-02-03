using System;
using System.Collections.Generic;
using Actions;
using Fusion;
using Grid;
using MoreMountains.Feedbacks;
using Units;
using UnityEngine;

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
    
    [Header("Feel Feedbacks")]
    public MMF_Player knifeFeedbackPlayer;
    
    [Networked] private bool IsKnifeAttacking { get; set;}
    [Networked] private NetworkId TargetUnitId { get; set; }
    [Networked] private bool IsRotating { get; set; }
    [Networked] private TickTimer DamageTimer { get; set; }
    
    private Unit _targetUnit;
    private Unit GetTargetUnit() => _targetUnit;
    public override string GetActionName() => "Knife";
    public bool GetIsKnifeAttacking() => IsKnifeAttacking;
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayKnifeFeedback()
    {
        if (knifeFeedbackPlayer != null)
        {
            knifeFeedbackPlayer.PlayFeedbacks();
        }
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = _unit.GetGridPosition();
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        foreach (var testPosition in ActionUtils.GetGridPositionsInRange(unitGridPosition, knifeAttackRange))
        {
            if (!LevelGrid.Instance.HasUnitAtGridPosition(testPosition)) continue;

            var unitsHere = LevelGrid.Instance.GetUnitAtGridPosition(testPosition);

            foreach (Unit potentialTarget in unitsHere)
            {
                if (!potentialTarget.Object || !potentialTarget.Object.IsInSimulation) continue;
                if (potentialTarget == _unit) continue;

                if (potentialTarget.GetTeamID() != _unit.GetTeamID())
                {
                    Vector3 attackerPos = _unit.GetWorldPosition() + Vector3.up * shoulderHeight;
                    Vector3 targetPos = potentialTarget.GetWorldPosition() + Vector3.up * shoulderHeight;
                    Vector3 stabDir = (targetPos - attackerPos).normalized;
                    float distanceToTarget = Vector3.Distance(attackerPos, targetPos);

                    if (Physics.Raycast(attackerPos, stabDir, distanceToTarget, obstacleLayerMask)) continue;
                }
                validGridPositionList.Add(testPosition);
                break;
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

        if (_unit.IsBusy || IsKnifeAttacking)
        {
            onActionComplete?.Invoke();
            return;
        }

        List<Unit> unitAtPos = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        if (unitAtPos.Count > 0)
        {
            Unit candidateTarget = unitAtPos[0];

            if (candidateTarget == _unit || candidateTarget.GetTeamID() == _unit.GetTeamID())
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
        IsRotating = true;
        IsKnifeAttacking = false;
        OnStartKnifeAttack?.Invoke(this, EventArgs.Empty);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
    
        if (_targetUnit == null || !_targetUnit.Object || !_targetUnit.Object.IsInSimulation)
        {
            if (TargetUnitId != default)
            {
                Debug.Log("Target is gone, aborting Knife Attack");
                IsKnifeAttacking = false;
                IsRotating = false;
                ActionComplete();
            }
            return;
        }

        if (IsRotating)
        {
            Vector3 directionToTarget = (_targetUnit.transform.position - transform.position).normalized;
            transform.forward = Vector3.Slerp(transform.forward, directionToTarget, rotationSpeed * Runner.DeltaTime);

            if (Vector3.Dot(transform.forward, directionToTarget) > 0.99f)
            {
                IsRotating = false;
                IsKnifeAttacking = true;
                DamageTimer = TickTimer.CreateFromSeconds(Runner, damageDelay);
            }
        }
        else if (IsKnifeAttacking)
        {
            if (DamageTimer.Expired(Runner))
            {
                HandleKnifeAttack();
            }
        }
    }

    private void HandleKnifeAttack()
    {
        if (_targetUnit == null || !_targetUnit.Object || !_targetUnit.Object.IsInSimulation)
        {
            Debug.LogWarning("Target is no longer valid, aborting knife attack");
            IsKnifeAttacking = false;
            ActionComplete();
            return;
        }
        Vector3 direction = (_targetUnit.GetWorldPosition() - _unit.GetWorldPosition()).normalized;
        _targetUnit.Damage(knifeDamageAmount);
        RPC_PlayKnifeFeedback();
        IsKnifeAttacking = false;
        ActionComplete();
    }

    public void StopKnifeAttack()
    {
        if (!IsKnifeAttacking)
        {
            RPC_StopKnifeAttack();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_StopKnifeAttack()
    {
        OnStopKnifeAttack?.Invoke(this, EventArgs.Empty);
        IsKnifeAttacking = false;
        ActionComplete();
    }
}
