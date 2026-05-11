using System.Collections.Generic;
using UnityEngine;
using MarbleRace.Core.Betting;
using MarbleRace.Core.Economy;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.Managers
{
    public class LiveBetManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float liveBetWindowEnd = 0.5f; // closes at 50% race progress
        [SerializeField] private float baseFeePercent = 0.2f;   // 20% fee at race start
        [SerializeField] private float maxFeePercent = 0.8f;    // 80% fee near window close

        [Header("References")]
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private BettingManager bettingManager;
        [SerializeField] private EconomyManager economyManager;

        private bool _liveBetUsed;
        private bool _windowOpen;
        private string _originalMarbleId;
        private int _originalBetAmount;

        public bool IsWindowOpen => _windowOpen && !_liveBetUsed;
        public bool HasUsedLiveBet => _liveBetUsed;

        /// <summary>
        /// Race progress from 0 to 1 based on leading marble's z position.
        /// Track is 80m long, finish at z=78.
        /// </summary>
        public float RaceProgress
        {
            get
            {
                if (raceManager == null || !raceManager.IsRacing) return 0f;
                float maxZ = 0f;
                foreach (var marble in raceManager.ActiveMarbles)
                {
                    if (marble != null && marble.transform.position.z > maxZ)
                        maxZ = marble.transform.position.z;
                }
                return Mathf.Clamp01(maxZ / 78f);
            }
        }

        /// <summary>
        /// Current fee percentage based on race progress.
        /// Scales linearly from baseFeePercent to maxFeePercent as the window closes.
        /// </summary>
        public float CurrentFeePercent
        {
            get
            {
                float progress = RaceProgress;
                float t = Mathf.Clamp01(progress / liveBetWindowEnd);
                return Mathf.Lerp(baseFeePercent, maxFeePercent, t);
            }
        }

        /// <summary>
        /// Get live odds based on current marble positions.
        /// Leading marbles get worse odds, trailing marbles get better odds.
        /// </summary>
        public Dictionary<string, float> GetLiveOdds()
        {
            var marbles = raceManager.ActiveMarbles;
            if (marbles == null || marbles.Count == 0)
                return new Dictionary<string, float>();

            // Sort by position (z) to get current ranking
            var sorted = new List<MarbleController>(marbles);
            sorted.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));

            var liveOdds = new Dictionary<string, float>();
            int count = sorted.Count;

            for (int i = 0; i < count; i++)
            {
                var identity = sorted[i].GetComponent<MarbleIdentity>();
                if (identity == null) continue;

                // Position-based odds: leader gets low odds, last place gets high odds
                // Range from 1.5x (1st) to 8x (last)
                float positionFactor = (float)i / (count - 1); // 0 for first, 1 for last
                float odds = Mathf.Lerp(1.5f, 8f, positionFactor);
                liveOdds[identity.MarbleId] = odds;
            }

            return liveOdds;
        }

        public void ResetForNewRace()
        {
            _liveBetUsed = false;
            _windowOpen = false;
            _originalMarbleId = null;
            _originalBetAmount = 0;
        }

        public void OpenWindow(string originalMarbleId, int originalBetAmount)
        {
            _originalMarbleId = originalMarbleId;
            _originalBetAmount = originalBetAmount;
            _windowOpen = true;
            _liveBetUsed = false;
        }

        public void CloseWindow()
        {
            _windowOpen = false;
        }

        private void Update()
        {
            if (!_windowOpen || _liveBetUsed) return;

            // Auto-close when race passes the window threshold
            if (RaceProgress >= liveBetWindowEnd)
            {
                _windowOpen = false;
            }
        }

        /// <summary>
        /// Double down: add more coins to the SAME marble bet.
        /// Cost = additionalBet + (additionalBet * currentFee)
        /// </summary>
        public LiveBetResult DoubleDown(int additionalBet)
        {
            if (!IsWindowOpen)
                return LiveBetResult.WindowClosed;

            if (string.IsNullOrEmpty(_originalMarbleId))
                return LiveBetResult.NoBetPlaced;

            float fee = CurrentFeePercent;
            int totalCost = additionalBet + Mathf.RoundToInt(additionalBet * fee);

            if (economyManager.Balance < totalCost)
                return LiveBetResult.InsufficientFunds;

            // Deduct the full cost (bet + fee)
            economyManager.SpendCoins(totalCost);

            // Add the additional bet to the pool (fee is house profit)
            var bet = new Bet("local_player", _originalMarbleId, additionalBet,
                System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            bettingManager.CurrentPool.AddBet(bet);

            _liveBetUsed = true;
            _windowOpen = false;

            HapticManager.MediumImpact();
            AudioManager.Instance?.PlayBetPlaced();

            return LiveBetResult.Success;
        }

        /// <summary>
        /// Switch bet: forfeit a percentage of original bet, place new bet on different marble.
        /// Forfeit = originalBet * 0.5 (always lose half your original)
        /// New bet = remaining half, placed at live odds on new marble.
        /// </summary>
        public LiveBetResult SwitchBet(string newMarbleId)
        {
            if (!IsWindowOpen)
                return LiveBetResult.WindowClosed;

            if (string.IsNullOrEmpty(_originalMarbleId))
                return LiveBetResult.NoBetPlaced;

            if (newMarbleId == _originalMarbleId)
                return LiveBetResult.SameMarble;

            // Forfeit 50% of original bet (already spent), transfer remaining to new marble
            int switchAmount = Mathf.RoundToInt(_originalBetAmount * 0.5f);

            // Add new bet to pool for the new marble
            var bet = new Bet("local_player_switch", newMarbleId, switchAmount,
                System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            bettingManager.CurrentPool.AddBet(bet);

            _originalMarbleId = newMarbleId;
            _liveBetUsed = true;
            _windowOpen = false;

            HapticManager.MediumImpact();
            AudioManager.Instance?.PlayBetPlaced();

            return LiveBetResult.Success;
        }

        /// <summary>
        /// Get the cost breakdown for a double-down bet.
        /// </summary>
        public void GetDoubleDownCost(int additionalBet, out int totalCost, out int feeAmount)
        {
            float fee = CurrentFeePercent;
            feeAmount = Mathf.RoundToInt(additionalBet * fee);
            totalCost = additionalBet + feeAmount;
        }

        /// <summary>
        /// Get potential payout for a double-down based on live odds.
        /// </summary>
        public int GetDoubleDownPotentialWin(int additionalBet)
        {
            var liveOdds = GetLiveOdds();
            if (liveOdds.ContainsKey(_originalMarbleId))
            {
                int totalBet = _originalBetAmount + additionalBet;
                return Mathf.RoundToInt(totalBet * liveOdds[_originalMarbleId]);
            }
            return 0;
        }

        /// <summary>
        /// Get potential payout for switching to a new marble.
        /// </summary>
        public int GetSwitchPotentialWin(string newMarbleId)
        {
            var liveOdds = GetLiveOdds();
            if (liveOdds.ContainsKey(newMarbleId))
            {
                int switchAmount = Mathf.RoundToInt(_originalBetAmount * 0.5f);
                return Mathf.RoundToInt(switchAmount * liveOdds[newMarbleId]);
            }
            return 0;
        }

        public string OriginalMarbleId => _originalMarbleId;
        public int OriginalBetAmount => _originalBetAmount;
    }

    public enum LiveBetResult
    {
        Success,
        WindowClosed,
        NoBetPlaced,
        InsufficientFunds,
        SameMarble
    }
}
