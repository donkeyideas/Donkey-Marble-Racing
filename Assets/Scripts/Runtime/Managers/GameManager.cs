using System.Collections;
using UnityEngine;
using MarbleRace.Core.Economy;
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
            StartCoroutine(DelayedResults());
        }

        private IEnumerator DelayedResults()
        {
            // Let the player watch the top-down finish camera for a moment
            yield return new WaitForSeconds(2.5f);
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
                    Time.fixedDeltaTime = 0.02f;
                    raceManager.EndRace();
                    RecordRaceStats();
                    break;
            }
        }

        private void RecordRaceStats()
        {
            if (RaceStatsManager.Instance == null || raceManager.LastResult == null) return;

            var result = raceManager.LastResult;
            var pool = bettingManager.CurrentPool;

            // Determine if player bet won
            bool betWon = false;
            int coinsChange = 0;

            if (pool != null && pool.TotalPool > 0)
            {
                var payouts = bettingManager.ResolveBets(result.WinnerMarbleId);
                foreach (var payout in payouts)
                {
                    if (payout.Won)
                    {
                        betWon = true;
                        coinsChange = payout.Payout;
                        // Add winnings to wallet
                        if (economyManager != null)
                            economyManager.AddCoins(payout.Payout);
                    }
                    else
                    {
                        coinsChange = -payout.BetAmount;
                    }
                }
            }

            RaceStatsManager.Instance.RecordRaceResult(
                result.WinnerMarbleId,
                result.FinishOrder,
                result.RaceDuration,
                betWon,
                coinsChange
            );
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
