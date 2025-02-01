using Fusion;
using UnityEngine;

namespace DestructibleObjects
{
    public class DestructionManager : NetworkBehaviour
    {
        public static DestructionManager Instance { get; private set; }

        [SerializeField] private GameObject fracturedCratePrefab;

        public override void Spawned()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Runner.Despawn(Object);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SpawnFracturedEffect(Vector3 pos, Quaternion rot)
        {
            Debug.Log($"[DestructionManager] Spawning fractured effect at {pos} on {Runner.LocalPlayer}");
            if (fracturedCratePrefab != null)
            {
                GameObject fracturedInstance = Instantiate(fracturedCratePrefab, pos, rot);
                FracturedExplosion fracturedExplosion = fracturedInstance.GetComponent<FracturedExplosion>();
                if (fracturedExplosion != null)
                {
                    fracturedExplosion.InitializeExplosion();
                }
            }
            else
            {
                Debug.LogError("DestructionManager: fracturedCratePrefab is not assigned");
            }
        }
    }
}