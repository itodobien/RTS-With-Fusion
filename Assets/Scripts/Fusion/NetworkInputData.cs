using UnityEngine;

namespace Fusion
{
    public struct NetworkInputData : INetworkInput
    {
        public const byte MOUSEBUTTON0 = 1;
        public const byte MOUSEBUTTON1 = 2;
        public Vector3 direction;
        public Vector3 mousePosition;
        public bool spawnUnit;
        public NetworkButtons buttons;
        public Vector3 targetPosition;
    }
}