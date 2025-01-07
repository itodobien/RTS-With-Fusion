using Fusion;
using UnityEngine;

namespace Units
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "RTS/UnitData", order = 0)]
    public class UnitData : ScriptableObject
    {
        [Header("Prefabs")]
        public NetworkPrefabRef liveUnitPrefab;   // The normal, playable unit
        public GameObject ragdollPrefab;    // The ragdoll or corpse prefab

        [Header("Meta Data")]
        public string unitName;
        public Sprite icon;
        public Color teamColorOverride;  // Example: if you want a color per unit type
    }
}