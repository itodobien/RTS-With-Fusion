using UnityEngine;

namespace Unit_Activities
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