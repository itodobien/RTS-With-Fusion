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
                    shootAction.OnShoot += ShootAction_OnShoot;
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
                    shootAction.OnShoot -= ShootAction_OnShoot;
                }
            }
        }

        private void Update()
        {
            if (_moveAction == null) return;
            bool isCurrentlyMoving = _moveAction.GetIsMoving();
        
            animator.SetBool(IsWalking, isCurrentlyMoving);
        }

        private void MoveAction_OnStartMoving(object sender, EventArgs e)
        {
            animator.SetBool(IsWalking, true);
        }

        private void MoveAction_OnStopMoving(object sender, EventArgs e)
        {
            animator.SetBool(IsWalking, false);
        }

        private void ShootAction_OnShoot(object sender, EventArgs e)
        {
            networkMecanimAnimator.SetTrigger(Shoot, false);
        }
    }
}
