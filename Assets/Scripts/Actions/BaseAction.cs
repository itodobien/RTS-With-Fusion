using Units;
using UnityEngine;

namespace Actions
{
    public class BaseAction : MonoBehaviour
    {
        protected Unit unit;
        protected bool isActive;

        protected virtual void Awake()
        {
            unit = GetComponent<Unit>();
        }
    }
}