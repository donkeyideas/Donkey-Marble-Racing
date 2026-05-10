using UnityEngine;
using MarbleRace.Events;

namespace MarbleRace.Runtime.Managers
{
    public enum GameState
    {
        MainMenu,
        Betting,
        Countdown,
        Racing,
        PhotoFinish,
        Results
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Events")]
        [SerializeField] private GameEvent onStateChanged;
        [SerializeField] private GameEvent onRaceRequested;

        [Header("References")]
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private BettingManager bettingManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private UIManager uiManager;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Claim daily reward on startup
            if (economyManager != null)
                economyManager.ClaimDailyReward();

            TransitionTo(GameState.MainMenu);
        }

        public void RequestNewRace()
        {
            if (CurrentState == GameState.MainMenu || CurrentState == GameState.Results)
            {
                TransitionTo(GameState.Betting);
            }
        }

        public void OnBettingComplete()
        {
            TransitionTo(GameState.Countdown);
        }

        public void OnCountdownComplete()
        {
            TransitionTo(GameState.Racing);
        }

        public void OnRaceFinished()
        {
            TransitionTo(GameState.Results);
        }

        public void OnPhotoFinish()
        {
            TransitionTo(GameState.PhotoFinish);
        }

        public void ReturnToMenu()
        {
            TransitionTo(GameState.MainMenu);
        }

        private void TransitionTo(GameState newState)
        {
            ExitState(CurrentState);
            CurrentState = newState;
            EnterState(newState);
            onStateChanged?.Raise();

            // Directly notify UI
            if (uiManager != null)
                uiManager.OnGameStateChanged();
        }

        private void EnterState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    break;
                case GameState.Betting:
                    bettingManager.OpenBetting();
                    raceManager.PrepareRace();
                    break;
                case GameState.Countdown:
                    bettingManager.CloseBetting();
                    raceManager.StartCountdown();
                    break;
                case GameState.Racing:
                    raceManager.StartRace();
                    break;
                case GameState.PhotoFinish:
                    Time.timeScale = 0.3f;
                    break;
                case GameState.Results:
                    Time.timeScale = 1f;
                    raceManager.EndRace();
                    break;
            }
        }

        private void ExitState(GameState state)
        {
            switch (state)
            {
                case GameState.PhotoFinish:
                    Time.timeScale = 1f;
                    break;
            }
        }
    }
}
