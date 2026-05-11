using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarbleRace.Core.Race;
using MarbleRace.Data;
using MarbleRace.Events;
using MarbleRace.Runtime.Marble;
using MarbleRace.Runtime.Track;
using MarbleRace.Runtime.UI;
using MarbleRace.Runtime.Camera;

namespace MarbleRace.Runtime.Managers
{
    public class RaceManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private RaceSettings raceSettings;
        [SerializeField] private TrackData currentTrack;
        [SerializeField] private PhysicsMaterial trackPhysicsMaterial;

        [Header("References")]
        [SerializeField] private MarbleSpawner marbleSpawner;
        [SerializeField] private StartGate startGate;
        [SerializeField] private FinishLine finishLine;
        [SerializeField] private RaceHUD raceHUD;
        [SerializeField] private RaceCamera raceCamera;
        [SerializeField] private WinCelebration winCelebration;

        [Header("Events")]
        [SerializeField] private GameEvent onRacePrepared;
        [SerializeField] private GameEvent onCountdownStarted;
        [SerializeField] private GameEvent onRaceStarted;
        [SerializeField] private GameEvent onRaceFinished;
        [SerializeField] private MarbleEvent onMarbleFinished;
        [SerializeField] private IntEvent onCountdownTick;

        private List<MarbleController> _activeMarbles = new List<MarbleController>();
        private List<string> _finishOrder = new List<string>();
        private float _raceTimer;
        private bool _isRacing;
        private bool _firstFinishTriggered;
        private Coroutine _countdownCoroutine;
        private GameObject _currentTrackObject;
        private TrackType _currentTrackType;

        public RaceResult LastResult { get; private set; }
        public List<MarbleController> ActiveMarbles => _activeMarbles;
        public List<string> FinishOrder => _finishOrder;
        public bool IsRacing => _isRacing;
        public float RaceTime => _raceTimer;
        public string CurrentTrackName => RuntimeTrackBuilder.GetTrackName(_currentTrackType);

        public void PrepareRace()
        {
            // Clean up previous race
            marbleSpawner.DespawnAll();
            _finishOrder.Clear();
            _raceTimer = 0f;
            _isRacing = false;
            _firstFinishTriggered = false;

            // Rebuild track with a random type
            RebuildTrack();

            // Close the start gate
            if (startGate != null)
                startGate.Close();

            // Reset finish line for new race
            if (finishLine != null)
                finishLine.ResetForNewRace();

            // Spawn new marbles
            _activeMarbles = marbleSpawner.SpawnMarbles(raceSettings.marbleCount, currentTrack);

            // Freeze marbles at start
            foreach (var marble in _activeMarbles)
                marble.Freeze();

            onRacePrepared?.Raise();
        }

        private void RebuildTrack()
        {
            // Destroy old track
            if (_currentTrackObject != null)
                Destroy(_currentTrackObject);

            // Also destroy any leftover track from the editor setup
            var existingTrack = GameObject.Find("Track");
            if (existingTrack != null)
                Destroy(existingTrack);

            // Pick a random track type
            var types = new[] { TrackType.Downhill, TrackType.Zigzag, TrackType.Funnel, TrackType.Spiral, TrackType.MultiPath };
            _currentTrackType = types[Random.Range(0, types.Length)];

            // Build the new track
            _currentTrackObject = RuntimeTrackBuilder.BuildTrack(_currentTrackType, trackPhysicsMaterial);
        }

        public void StartCountdown()
        {
            _countdownCoroutine = StartCoroutine(CountdownRoutine());
        }

        public void StartRace()
        {
            // Open the start gate
            if (startGate != null)
                startGate.Open();

            // Release all marbles
            foreach (var marble in _activeMarbles)
                marble.Release();

            _isRacing = true;
            onRaceStarted?.Raise();

            // Hide countdown on HUD
            if (raceHUD != null)
                raceHUD.HideCountdown();

            // Tell camera to follow marbles
            if (raceCamera != null)
                raceCamera.SetMarbles(_activeMarbles);

            // Start crowd ambience
            AudioManager.Instance?.StartCrowdAmbience();
        }

        public void EndRace()
        {
            _isRacing = false;

            // Restore normal time (celebration sets slow-mo)
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            // Stop crowd ambience, play cheer
            AudioManager.Instance?.StopCrowdAmbience();
            AudioManager.Instance?.PlayCrowdCheer();

            // Reset camera
            if (raceCamera != null)
                raceCamera.ResetFinishMode();

            // Build result
            string winner = _finishOrder.Count > 0 ? _finishOrder[0] : GetLeadingMarbleId();

            // Fill in remaining positions for marbles that didn't cross finish
            foreach (var marble in _activeMarbles)
            {
                if (!_finishOrder.Contains(marble.MarbleId))
                    _finishOrder.Add(marble.MarbleId);
            }

            LastResult = new RaceResult(winner, new List<string>(_finishOrder), _raceTimer, _raceTimer >= raceSettings.raceTimeout);
            onRaceFinished?.Raise();
        }

        public void RegisterFinish(MarbleController marble)
        {
            // Legacy — kept for API compatibility, detection now in Update
            if (_finishOrder.Contains(marble.MarbleId)) return;
            if (marble.HasFinished) return;

            marble.MarkFinished();
            _finishOrder.Add(marble.MarbleId);
            onMarbleFinished?.Raise(marble.gameObject);
        }

        private void Update()
        {
            if (!_isRacing) return;

            _raceTimer += Time.deltaTime;

            // Position-based finish detection — any marble past z=78 is in the bucket zone
            foreach (var marble in _activeMarbles)
            {
                if (marble == null || marble.HasFinished) continue;
                if (marble.transform.position.z >= 78f)
                {
                    marble.MarkFinished();
                    _finishOrder.Add(marble.MarbleId);
                    onMarbleFinished?.Raise(marble.gameObject);

                    if (!_firstFinishTriggered)
                    {
                        _firstFinishTriggered = true;
                        if (raceCamera != null)
                            raceCamera.FocusOnMarble(marble.transform);
                        if (winCelebration != null)
                            winCelebration.Play(marble.transform.position);
                        AudioManager.Instance?.PlayCrowdCheer();
                    }

                    if (_finishOrder.Count >= _activeMarbles.Count)
                    {
                        GameManager.Instance.OnRaceFinished();
                        return;
                    }
                }
            }

            if (_raceTimer >= raceSettings.raceTimeout)
            {
                GameManager.Instance.OnRaceFinished();
            }
        }

        private string GetLeadingMarbleId()
        {
            // Fallback: return marble closest to the finish line (furthest along Z axis)
            MarbleController leader = null;
            float maxProgress = float.MinValue;

            foreach (var marble in _activeMarbles)
            {
                float progress = marble.transform.position.z;
                if (progress > maxProgress)
                {
                    maxProgress = progress;
                    leader = marble;
                }
            }

            return leader != null ? leader.MarbleId : "";
        }

        private IEnumerator CountdownRoutine()
        {
            onCountdownStarted?.Raise();
            int count = (int)raceSettings.countdownDuration;

            for (int i = count; i > 0; i--)
            {
                onCountdownTick?.Raise(i);
                if (raceHUD != null)
                    raceHUD.ShowCountdown(i);
                yield return new WaitForSeconds(1f);
            }

            onCountdownTick?.Raise(0);
            if (raceHUD != null)
                raceHUD.ShowCountdown(0); // Shows "GO!"

            yield return new WaitForSeconds(0.5f);
            GameManager.Instance.OnCountdownComplete();
        }
    }
}
