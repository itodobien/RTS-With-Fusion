using System;
using Actions;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Units
{
    public class UnitAnimator : NetworkBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private NetworkMecanimAnimator networkMecanimAnimator;
        [SerializeField] private GameObject rifleGameObject;
        [SerializeField] private GameObject knifeGameObject;
        
        [Networked] private bool IsKnifeEquipped { get; set; }
        
        private MoveAction _moveAction;
        private ShootAction _shootAction;
        private DanceAction _danceAction;
        private KnifeAction _knifeAction;
    
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int Shoot = Animator.StringToHash("Shoot");
        private static readonly int IsDancing = Animator.StringToHash("IsDancing");
        private static readonly int IsKnifeAttacking = Animator.StringToHash("IsKnifeAttacking");
        
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
                    shootAction.OnStartShooting += ShootAction_OnStartShooting;
                    shootAction.OnStopShooting += ShootAction_OnStopShooting;
                }
                else if (action is DanceAction danceAction)
                {
                    _danceAction = danceAction;
                    danceAction.OnStartDancing += DanceAction_OnStartDancing;
                    danceAction.OnStopDancing += DanceAction_OnStopDancing;
                }
                else if (action is KnifeAction knifeAction)
                {
                    _knifeAction = knifeAction;
                    knifeAction.OnStartKnifeAttack += KnifeAction_OnStartKnifeAttack;
                    knifeAction.OnStopKnifeAttack += KnifeAction_OnStopKnifeAttack;
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
                    shootAction.OnStartShooting -= ShootAction_OnStartShooting;
                    shootAction.OnStopShooting -= ShootAction_OnStopShooting;
                }
                else if (action is DanceAction danceAction)
                {
                    danceAction.OnStartDancing -= DanceAction_OnStartDancing;
                    danceAction.OnStopDancing -= DanceAction_OnStopDancing;
                }
                else if (action is KnifeAction knifeAction)
                {
                    _knifeAction = knifeAction;
                    knifeAction.OnStartKnifeAttack -= KnifeAction_OnStartKnifeAttack;
                    knifeAction.OnStopKnifeAttack -= KnifeAction_OnStopKnifeAttack;
                }
            }
        }

        private void Start()
        {
            EquipRifle();
        }
        
        public override void Render()
        {
            base.Render();
            UpdateWeaponVisibility();
        }
        
        private void UpdateWeaponVisibility()
        {
            rifleGameObject.SetActive(!IsKnifeEquipped);
            knifeGameObject.SetActive(IsKnifeEquipped);
        }

        private void Update()
        {
            if (_moveAction != null)
            {
                bool isCurrentlyMoving = _moveAction.GetIsMoving();
                animator.SetBool(IsWalking, isCurrentlyMoving);
            }
            else
            {
                animator.SetBool(IsWalking, false);
            }

            if (_shootAction != null)
            {
                bool isCurrentlyShooting = _shootAction.GetIsFiring(); 
                animator.SetBool(Shoot, isCurrentlyShooting);
            }
            else
            {
                animator.SetBool(Shoot, false);
            }

            if (_danceAction != null)
            {
                bool isCurrentlyDancing = _danceAction.GetIsDancing();
                animator.SetBool(IsDancing, isCurrentlyDancing);
            }
            else
            {
                animator.SetBool(IsDancing, false);
            }

            if (_knifeAction != null)
            {
                bool isCurrentlyKnifeAttacking = _knifeAction.GetIsKnifeAttacking();
                animator.SetBool(IsKnifeAttacking, isCurrentlyKnifeAttacking);
            }
            else
            {
                animator.SetBool(IsKnifeAttacking, false);
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

        private void ShootAction_OnStartShooting(object sender, EventArgs e)
        {
            animator.SetBool(Shoot, true);
        }
        
        private void ShootAction_OnStopShooting(object sender, EventArgs e)
        {
            animator.SetBool(Shoot, false);
        }

        private void DanceAction_OnStartDancing(object sender, EventArgs e)
        {
            animator.SetBool(IsDancing, true);
        }
        
        private void DanceAction_OnStopDancing(object sender, EventArgs e)
        {
            animator.SetBool(IsDancing, false);
        }

        private void KnifeAction_OnStartKnifeAttack(object sender, EventArgs e)
        {
            EquipKnife();
            animator.SetBool(IsKnifeAttacking, true);
        }

        private void KnifeAction_OnStopKnifeAttack(object sender, EventArgs e)
        {
            IsKnifeEquipped = false;
            EquipRifle();
            UpdateWeaponVisibility();
            animator.SetBool(IsKnifeAttacking, false);
        }

        private void EquipKnife()
        {
            IsKnifeEquipped = true;
        }
        private void EquipRifle()
        {
            IsKnifeEquipped = false;

        }
    }
}
