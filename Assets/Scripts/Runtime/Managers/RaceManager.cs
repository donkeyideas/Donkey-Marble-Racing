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

        [Header("References")]
        [SerializeField] private MarbleSpawner marbleSpawner;
        [SerializeField] private StartGate startGate;
        [SerializeField] private FinishLine finishLine;
        [SerializeField] private RaceHUD raceHUD;
        [SerializeField] private RaceCamera raceCamera;

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
        private Coroutine _countdownCoroutine;

        public RaceResult LastResult { get; private set; }
        public List<MarbleController> ActiveMarbles => _activeMarbles;
        public bool IsRacing => _isRacing;
        public float RaceTime => _raceTimer;

        public void PrepareRace()
        {
            // Clean up previous race
            marbleSpawner.DespawnAll();
            _finishOrder.Clear();
            _raceTimer = 0f;
            _isRacing = false;

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
        }

        public void EndRace()
        {
            _isRacing = false;

            // Restore normal time (celebration sets slow-mo)
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

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
            if (_finishOrder.Contains(marble.MarbleId)) return;

            _finishOrder.Add(marble.MarbleId);
            onMarbleFinished?.Raise(marble.gameObject);

            // End race once all marbles have finished
            if (_finishOrder.Count >= _activeMarbles.Count)
            {
                GameManager.Instance.OnRaceFinished();
            }
        }

        private void Update()
        {
            if (!_isRacing) return;

            _raceTimer += Time.deltaTime;

            if (_raceTimer >= raceSettings.raceTimeout)
            {
                // Timeout — determine positions by track progress
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
