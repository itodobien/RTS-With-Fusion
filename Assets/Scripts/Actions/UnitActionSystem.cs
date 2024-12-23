using Fusion;
using Grid;
using UnityEngine;

namespace Actions
{
    public class UnitActionSystem : NetworkBehaviour
    {
        public static UnitActionSystem Instance { get; private set; }
        
        public override void Spawned()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Multiple UAS instances detected. Destroying the new one.");
                Runner.Despawn(Object);
            }
            Instance = this;
        }

        
        public override void FixedUpdateNetwork()
        {
            //
        }
    }
}