using System;
using Actions;
using Fusion;
using UnityEngine;

namespace Units
{
    public class UnitAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private NetworkMecanimAnimator networkMecanimAnimator;
        private MoveAction _moveAction;
        private ShootAction _shootAction;
    
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int Shoot = Animator.StringToHash("Shoot");

        private void Awake()
        {
            var actions = GetComponents<BaseAction>();
            foreach (var action in actions)
            {
                if (action is MoveAction moveAction)
                {
                    _moveAction = moveAction;
                    moveAction.OnStartMoving += MoveAction_OnStartMoving;
                    moveAction.OnStopMoving  += MoveAction_OnStopMoving;
                }
                else if (action is ShootAction shootAction)
                {
                    _shootAction = shootAction;
                    shootAction.OnStartShooting += StartShootingActionOnStartShooting;
                    shootAction.OnStopShooting += ShootAction_OnStophooting;
                }
            }
        }

        private void OnDestroy()
        {
            var actions = GetComponents<BaseAction>();
            foreach (var action in actions)
            {
                if (action is MoveAction moveAction)
                {
                    moveAction.OnStartMoving -= MoveAction_OnStartMoving;
                    moveAction.OnStopMoving  -= MoveAction_OnStopMoving;
                }
                else if (action is ShootAction shootAction)
                {
                    shootAction.OnStartShooting -= StartShootingActionOnStartShooting;
                    shootAction.OnStopShooting -= ShootAction_OnStophooting;
                }
            }
        }

        private void Update()
        {
            if (_moveAction == null)
            {
                return;
            }
            if (_moveAction != null)
            {
                bool isCurrentlyMoving = _moveAction.GetIsMoving();
                animator.SetBool(IsWalking, isCurrentlyMoving);
            }
            if (_shootAction == null) return;
            if (_shootAction != null)
            {
                bool isCurrentlyShooting = _shootAction.GetIsFiring(); 
                animator.SetBool(Shoot, isCurrentlyShooting);
            }
        }

        private void MoveAction_OnStartMoving(object sender, EventArgs e)
        {
            animator.SetBool(IsWalking, true);
        }

        private void MoveAction_OnStopMoving(object sender, EventArgs e)
        {
            animator.SetBool(IsWalking, false);
        }

        private void StartShootingActionOnStartShooting(object sender, EventArgs e)
        {
            animator.SetBool(Shoot, true);
        }
        
        private void ShootAction_OnStophooting(object sender, EventArgs e)
        {
            animator.SetBool(Shoot, true);
        }
    }
}
