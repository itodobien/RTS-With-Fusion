using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Grid;
using Pathfinding;
using UnityEngine;

namespace Actions
{
    public class MoveAction : BaseAction
    {
        public event EventHandler OnStartMoving;
        public event EventHandler OnStopMoving;
        
        [Networked] private Vector3 TargetPosition { get; set; }
        [Networked] private PlayerRef OwnerPlayerRef { get; set; }
        [Networked] private bool IsMoving { get; set; }

        [Header("Move Action Settings")]
        [SerializeField] private float stopDistance = 0.1f;
        [SerializeField] private float rotateSpeed = 10f; 
        [SerializeField] private int maxMoveDistance = 4;
        [SerializeField] private float moveSpeed = 4f;
        
        public bool GetIsMoving() => IsMoving;

        [Header("A* Pathfinding Settings")]
        private Seeker _seeker;
        private Path _path;
        private int _currentWaypointIndex;
        
        [SerializeField] private float nextWaypointDistance = 3f;  

        public override string GetActionName() => "Move";

        public override void Spawned()
        {
            TargetPosition = transform.position;
            OwnerPlayerRef = Object.InputAuthority;

            _seeker = GetComponent<Seeker>();
            if (_seeker == null)
            {
                _seeker = gameObject.AddComponent<Seeker>();
            }
        }
        private void OnPathComplete(Path p)
        {
            if (p.error)
            {
                _path = null;
                return;
            }
            _path = p;
            _currentWaypointIndex = 0;
        }

        public override void FixedUpdateNetwork()
        {
            if (_unit == null || !_unit.Object || !_unit.Object.IsInSimulation)
            {
                ActionComplete();
                return;
            }
            if (_unit.IsBusy && !IsMoving) return;

            if (IsMoving && _path != null)
            {
                MoveUnitAlongPath();
            }
            else if (IsMoving && _path == null)
            {
                //might need to wait for path calc in here
            }
        }

        private void MoveUnitAlongPath()
        {
            if (_path.vectorPath == null || _path.vectorPath == null || _path.vectorPath.Count == 0)
            {
                StopMoving();
                return;
            }
            Vector3 finalDestination = _path.vectorPath[_path.vectorPath.Count - 1];
            float distanceToFinalDestination = Vector3.Distance(transform.position, finalDestination);
            if (distanceToFinalDestination <= stopDistance)
            {
                StopMoving();
                return;
            }

            while (_currentWaypointIndex < _path.vectorPath.Count - 1 &&
                   Vector3.Distance(transform.position, _path.vectorPath[_currentWaypointIndex]) < nextWaypointDistance)
            {
                _currentWaypointIndex++;
            }

            Vector3 currentWaypoint = _path.vectorPath[_currentWaypointIndex];
            Vector3 moveDirection = (currentWaypoint - transform.position).normalized;
            Vector3 velocity = moveDirection * moveSpeed;
            transform.position += velocity * Runner.DeltaTime;

            if (velocity.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(velocity, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotateSpeed * Runner.DeltaTime
                );
            }
        }

        private void StopMoving()
        {
            IsMoving = false;
            _path = null;
            ActionComplete();
            OnStopMoving?.Invoke(this, EventArgs.Empty);
        }

        private bool IsValidActionGridPosition(GridPosition gridPosition)
        {
            List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
            return validGridPositionList.Contains(gridPosition);
        }

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            GridPosition unitGridPosition = _unit.GetGridPosition();
            return ActionUtils.GetGridPositionsInRange(unitGridPosition, maxMoveDistance).Where(pos => pos != unitGridPosition 
                    && !LevelGrid.Instance.HasUnitAtGridPosition(pos)).ToList();
        }

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete = null)
        {
            if (!Object.HasStateAuthority)
            {
                onActionComplete?.Invoke();
                return;
            }
            if (!IsValidActionGridPosition(gridPosition))
            {
                onActionComplete?.Invoke();
                return;
            }
            StartAction(onActionComplete);
            OnStartMoving?.Invoke(this, EventArgs.Empty);

            IsMoving = true;

            TargetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);

            if (_seeker == null) _seeker = GetComponent<Seeker>();
            _seeker.StartPath(transform.position, TargetPosition, OnPathComplete);
        }
        public void ResetPath()
        {
            _path = null;
            _currentWaypointIndex = 0;

            if (IsMoving)
            {
                _seeker.CancelCurrentPathRequest();
                _seeker.StartPath(transform.position, TargetPosition, OnPathComplete);
            }
        }
    }
}
