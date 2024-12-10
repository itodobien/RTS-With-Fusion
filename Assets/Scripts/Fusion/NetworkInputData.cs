using UnityEngine;

namespace Fusion
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 direction;
        public Vector3 mousePosition;
        public bool spawnUnit;
    }
}