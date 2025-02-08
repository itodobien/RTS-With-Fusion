using System;
using Fusion;
using Grid;
using Integrations.Interfaces;
using UnityEngine;

public class InteractSphere : NetworkBehaviour, IInteractable
{
    [SerializeField] private Material material1;
    [SerializeField] private Material material2;
    [SerializeField] private float switchCooldown = 1f;
    private float lastSwitchTime;

    
    [Networked, OnChangedRender(nameof(OnMaterialChanged))]
    public NetworkBool UseMaterial1 { get; set; }

    private MeshRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponentInChildren<MeshRenderer>();
    }
    
    private void Start()
    {
        GridPosition gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetInteractableAtGridPosition(gridPosition, this);
    }

    
    public override void Spawned()
    {
        OnMaterialChanged();
    }

    public void OnMaterialChanged()
    {
        _renderer.material = UseMaterial1 ? material1 : material2;
    }

    public void SwitchMaterial()
    {
        if (!Runner.IsServer) return;
        
        if (Time.time - lastSwitchTime >= switchCooldown)
        {
            UseMaterial1 = !UseMaterial1;
            lastSwitchTime = Time.time;
        }

    }

    public void Interact(Action onInteractionComplete)
    {
        if (Runner.IsServer)
        {
            SwitchMaterial();
        }
        onInteractionComplete?.Invoke();
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestInteract()
    {
        Interact(null);
    }
}