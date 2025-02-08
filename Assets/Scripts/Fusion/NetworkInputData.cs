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
        Grenade = 5,
        Knife = 6,
        Interact = 7
    }
    public struct NetworkInputData : INetworkInput
    {
        public const byte Mousebutton0 = 0;
        public const byte Mousebutton1 = 1;
        public const byte SpawnUnit = 2;
        public const byte Jump = 3;
        public const byte Spin = 4;
        public const byte SelectUnit = 5;
        public const byte SwitchMaterial = 6;
        
        public Vector3 Direction;
        public NetworkButtons Buttons;
        public Vector3 TargetPosition;
        public Vector3 SpawnPosition;
        
        public int TargetGridX;
        public int TargetGridZ;
        
        public NetworkId SelectedUnitId;
        public bool IsSelected;
        
        public ActionType ActionType;
    }
    
}