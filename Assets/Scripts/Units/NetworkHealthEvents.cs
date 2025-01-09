using Fusion;
using UI;
using Units;
using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class NetworkHealthEvents : NetworkBehaviour
{
    private HealthSystem _healthSystem;

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
    }

    public override void Spawned()
    {
        _healthSystem.OnDamaged += HealthSystem_OnDamaged;
    }

    private void OnDestroy()
    {
        _healthSystem.OnDamaged -= HealthSystem_OnDamaged;
    }

    private void HealthSystem_OnDamaged(object sender, System.EventArgs e)
    {
        if (!Object.HasStateAuthority) return;
        float hpNormalized = _healthSystem.GetHealthNormalized();
        RPC_BroadcastHealthChanged(hpNormalized);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastHealthChanged(float normalizedHealth)
    {
        var worldUI = GetComponentInChildren<UnitWorldUI>();
        if (worldUI != null)
        {
            worldUI.SetHealthBar(normalizedHealth);
        }
    }
}