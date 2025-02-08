using System;
using DG.Tweening;
using Fusion;
using Grid;
using Integrations.Interfaces;
using Pathfinding;
using UnityEngine;

public class Door : NetworkBehaviour, IInteractable
{
    [Networked] private bool IsDoorOpen { get; set; }
    
    [SerializeField] private float rescanWalkableAreaTimer = 0.5f;
    
    private GraphUpdateScene _graphUpdateScene;
    private GridPosition _gridPosition;
    private bool _hasSpawned;
    
    private float _lastInteractTime = -Mathf.Infinity;
    private readonly float _interactCooldown = 0.5f;

    private static readonly int IsOpen = Animator.StringToHash("IsOpen");

    public override void Spawned()
    {
        _graphUpdateScene = GetComponent<GraphUpdateScene>();
        if (Object.HasStateAuthority)
        {
            IsDoorOpen = false;
        }
        _hasSpawned = true;
    }

    private void Start()
    {
        _gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetInteractableAtGridPosition(_gridPosition, this);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OpenDoor() => OpenDoor();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_CloseDoor() => CloseDoor();

    private void OpenDoor()
    {
        IsDoorOpen = true;
        DisableComponents();
        UpdateDoorVisuals();
        DOVirtual.DelayedCall(rescanWalkableAreaTimer, RescanWalkableArea);
    }

    private void CloseDoor()
    {
        IsDoorOpen = false;
        EnableComponents();
        UpdateDoorVisuals();
        DOVirtual.DelayedCall(rescanWalkableAreaTimer, RescanWalkableArea);
    }

    private void RescanWalkableArea()
    {
        if (AstarPath.active == null) return;
        if (_graphUpdateScene != null) _graphUpdateScene.Apply();
        else AstarPath.active.UpdateGraphs(new Bounds(transform.position, Vector3.one * 5f));
    }
    
    private void UpdateDoorVisuals()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.SetBool(IsOpen, IsDoorOpen);
    }
    
    private void DisableComponents()
    {
        foreach (var colliders in GetComponentsInChildren<Collider>())
            colliders.enabled = false;
        if (_graphUpdateScene != null) _graphUpdateScene.enabled = false;
    }

    private void EnableComponents()
    {
        foreach (var colliders in GetComponentsInChildren<Collider>())
            colliders.enabled = true;
        if (_graphUpdateScene != null) _graphUpdateScene.enabled = true;
    }

    public void Interact(Action onInteractionComplete)
    {
        if (!_hasSpawned) return;
        if (Time.time - _lastInteractTime < _interactCooldown) return;
        
        _lastInteractTime = Time.time;
        
        if (IsDoorOpen) RPC_CloseDoor();
        else RPC_OpenDoor();

        onInteractionComplete?.Invoke();
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestInteract()
    {
        Interact(null);
    }
}