using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MarbleRace.Data;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Track;

namespace MarbleRace.Runtime.UI
{
    public class TrackSelectionPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text selectedTrackText;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        private int _currentIndex = 0; // 0 = Random, 1..N = specific tracks
        private readonly TrackType[] _trackTypes = (TrackType[])System.Enum.GetValues(typeof(TrackType));

        // null = random, otherwise specific track
        public TrackType? SelectedTrack { get; private set; } = null;

        private void OnEnable()
        {
            if (prevButton != null) prevButton.onClick.AddListener(OnPrev);
            if (nextButton != null) nextButton.onClick.AddListener(OnNext);
            UpdateDisplay();
        }

        private void OnDisable()
        {
            if (prevButton != null) prevButton.onClick.RemoveListener(OnPrev);
            if (nextButton != null) nextButton.onClick.RemoveListener(OnNext);
        }

        private void OnPrev()
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _trackTypes.Length; // wrap to last specific track
            ApplySelection();
            AudioManager.Instance?.PlayButtonTap();
        }

        private void OnNext()
        {
            _currentIndex++;
            if (_currentIndex > _trackTypes.Length) _currentIndex = 0; // wrap to Random
            ApplySelection();
            AudioManager.Instance?.PlayButtonTap();
        }

        private void ApplySelection()
        {
            if (_currentIndex == 0)
                SelectedTrack = null;
            else
                SelectedTrack = _trackTypes[_currentIndex - 1];
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (selectedTrackText == null) return;

            if (_currentIndex == 0)
            {
                selectedTrackText.text = "RANDOM";
            }
            else
            {
                selectedTrackText.text = RuntimeTrackBuilder.GetTrackName(_trackTypes[_currentIndex - 1]);
            }
        }
    }
}
