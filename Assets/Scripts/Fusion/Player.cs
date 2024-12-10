namespace Fusion
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _characterController;

        private void Awake()
        {
            _characterController = GetComponent<NetworkCharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _characterController.Move(5*data.direction*Runner.DeltaTime);
            }
        }
    }
}