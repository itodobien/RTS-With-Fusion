#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    /// <summary>
    /// A wrapper for an EventBase object.<br />
    /// <br />
    /// Since Playmaker variables can not store arbitrary types we have to wrap event data
    /// in a UnityEngie.Object, see: https://forum.unity.com/threads/playmaker-visual-scripting-for-unity.72349/page-70#post-9271821
    /// </summary>
    public class EventObject : ScriptableObject, IEquatable<EventObject>
    {
        protected EventBase _event;
        public EventBase Event
        {
            get => _event;

            set
            {
                if (_event != value)
                {
                    _event = value;
                    refreshName();
                }
            }
        }

        public static EventObject CreateInstance(EventBase evt)
        {
            var obj = ScriptableObject.CreateInstance<EventObject>();
            obj.Event = evt;
            return obj;
        }

        protected void refreshName()
        {
            if (Event != null)
            {
                var target = Event.target as VisualElement;
                if (target != null && !string.IsNullOrEmpty(target.name))
                {
                    name = target.name + " (" + Event.GetType().Name + ")";
                    return;
                }
            }

            name = null;
        }

        public override bool Equals(object obj) => Equals(obj as EventObject);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Event.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(EventObject other)
        {
            return Event.Equals(other.Event);
        }
    }
}
#endif
