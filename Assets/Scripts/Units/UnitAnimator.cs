using System;
using Actions;
using UnityEngine;

namespace Units
{
    public class UnitAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private MoveAction _moveAction;
        private SpinAction _spinAction;
    
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");

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
                else if (action is SpinAction spinAction)
                {
                    spinAction.OnStartSpinning += SpinAction_OnStartSpinning;
                    spinAction.OnStopSpinning += SpinAction_OnStopSpinning;
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
                else if (action is SpinAction spinAction)
                {
                    spinAction.OnStartSpinning -= SpinAction_OnStartSpinning;
                    spinAction.OnStopSpinning  -= SpinAction_OnStopSpinning;
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

        private void SpinAction_OnStartSpinning(object sender, EventArgs e)
        {
            //
        }
        private void SpinAction_OnStopSpinning(object sender, EventArgs e)
        {
            //
        }
    }
}
