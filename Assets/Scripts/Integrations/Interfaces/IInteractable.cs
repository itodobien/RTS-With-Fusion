using System;
using UnityEngine;

namespace Integrations.Interfaces
{
    public interface IInteractable
    {
        public void Interact(Action onInteractionComplete);
    }
}
