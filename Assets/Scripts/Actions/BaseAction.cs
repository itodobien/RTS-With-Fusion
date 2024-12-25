using Fusion;
using Units;


namespace Actions
{
    public abstract class BaseAction : NetworkBehaviour
    {
        protected Unit _unit;
        protected bool isActive;

        protected virtual void Awake()
        {
            _unit = GetComponent<Unit>();
        }

        protected virtual void Start()
        {
            
        }
    }
}