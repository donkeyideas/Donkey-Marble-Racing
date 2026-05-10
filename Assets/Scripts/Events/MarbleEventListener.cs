using UnityEngine;
using UnityEngine.Events;

namespace MarbleRace.Events
{
    public class MarbleEventListener : MonoBehaviour
    {
        [SerializeField] private MarbleEvent _event;
        [SerializeField] private UnityEvent<GameObject> _response;

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

        public void OnEventRaised(GameObject marble)
        {
            _response?.Invoke(marble);
        }
    }
}
