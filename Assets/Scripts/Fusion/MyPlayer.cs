/*
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyPlayer : NetworkBehaviour // this class is just for the KCC demo. not really used in the game.
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpulse = 10f;
    
    [Networked] private NetworkButtons PreviousButtons { get; set; }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input)) //this is different from the example
        {
            Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.direction.x, 0f, input.direction.y);
            float jump = 0f;

            if (input.buttons.WasPressed(PreviousButtons, NetworkInputData.JUMP))
            {
                jump = jumpImpulse;
            }
            kcc.Move(worldDirection.normalized * speed, jump);
            PreviousButtons = input.buttons;

        }
    }
}
*/
