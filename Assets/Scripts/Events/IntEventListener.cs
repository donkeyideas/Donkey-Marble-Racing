using UnityEngine;
using UnityEngine.Events;

namespace MarbleRace.Events
{
    public class IntEventListener : MonoBehaviour
    {
        [SerializeField] private IntEvent _event;
        [SerializeField] private UnityEvent<int> _response;

        private void OnEnable()
        {
            if (_event != null)
                _event.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (_event != null)
                _event.UnregisterListener(this);
        }

        public void OnEventRaised(int value)
        {
            _response?.Invoke(value);
        }
    }
}
