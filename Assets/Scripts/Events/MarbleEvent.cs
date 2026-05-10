using System.Collections.Generic;
using UnityEngine;

namespace MarbleRace.Events
{
    [CreateAssetMenu(menuName = "MarbleRace/Events/Marble Event")]
    public class MarbleEvent : ScriptableObject
    {
        private readonly List<MarbleEventListener> _listeners = new List<MarbleEventListener>();

        public void Raise(GameObject marble)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i].OnEventRaised(marble);
        }

        public void RegisterListener(MarbleEventListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnregisterListener(MarbleEventListener listener)
        {
            _listeners.Remove(listener);
        }
    }
}
