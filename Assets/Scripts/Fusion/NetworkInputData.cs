using Grid;
using UnityEngine;

namespace Fusion
{
    public struct NetworkInputData : INetworkInput
    {
        public const byte MOUSEBUTTON0 = 0;
        public const byte MOUSEBUTTON1 = 1;
        public const byte SPAWNUNIT = 2;
        public const byte JUMP = 3;
        public const byte SELECT_UNIT = 0X10;

        public Vector3 direction;
        public NetworkButtons buttons;
        public Vector3 targetPosition;
        public Vector3 spawnPosition;

        public NetworkId selectedUnitId;
        public bool isSelected;
        
    }
}