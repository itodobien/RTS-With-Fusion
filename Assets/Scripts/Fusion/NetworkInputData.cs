using UnityEngine;

namespace Fusion
{
    
    public enum ActionType
    {
        None = 0,
        Move = 1,
        Spin = 2,
        Shoot = 3,
        Dance = 4,
        Grenade = 5
    }
    public struct NetworkInputData : INetworkInput
    {
        public const byte MOUSEBUTTON0 = 0;
        public const byte MOUSEBUTTON1 = 1;
        public const byte SPAWNUNIT = 2;
        public const byte JUMP = 3;
        public const byte SPIN = 4;
        public const byte SELECT_UNIT = 5;
        
        public Vector3 direction;
        public NetworkButtons buttons;
        public Vector3 targetPosition;
        public Vector3 spawnPosition;
        
        public int targetGridX;
        public int targetGridZ;
        
        public NetworkId selectedUnitId;
        public bool isSelected;
        
        public ActionType actionType;
    }
    
}