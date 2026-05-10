using System.Collections.Generic;
using UnityEngine;

namespace MarbleRace.Events
{
    [CreateAssetMenu(menuName = "MarbleRace/Events/Int Event")]
    public class IntEvent : ScriptableObject
    {
        private readonly List<IntEventListener> _listeners = new List<IntEventListener>();

        public void Raise(int value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i].OnEventRaised(value);
        }

        public void RegisterListener(IntEventListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnregisterListener(IntEventListener listener)
        {
            _listeners.Remove(listener);
        }
    }
}
