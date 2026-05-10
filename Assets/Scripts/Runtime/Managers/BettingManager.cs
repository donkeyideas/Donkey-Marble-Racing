using System.Collections.Generic;
using UnityEngine;
using MarbleRace.Core.Betting;
using MarbleRace.Core.Economy;
using MarbleRace.Data;
using MarbleRace.Events;

namespace MarbleRace.Runtime.Managers
{
    public class BettingManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EconomyConfig economyConfig;

        [Header("Events")]
        [SerializeField] private GameEvent onBettingOpened;
        [SerializeField] private GameEvent onBettingClosed;
        [SerializeField] private GameEvent onBetPlaced;
        [SerializeField] private IntEvent onPayoutReceived;

        private BettingPool _currentPool;
        private BetValidator _validator;
        private bool _isBettingOpen;

        public BettingPool CurrentPool => _currentPool;
        public bool IsBettingOpen => _isBettingOpen;

        private void Awake()
        {
            _currentPool = new BettingPool();
            int minBet = economyConfig != null ? economyConfig.minimumBet : 10;
            int maxBet = economyConfig != null ? economyConfig.maximumBet : 500;
            _validator = new BetValidator(minBet, maxBet);
        }

        public void OpenBetting()
        {
            _currentPool.Clear();
            _isBettingOpen = true;
            onBettingOpened?.Raise();
        }

        public void CloseBetting()
        {
            _isBettingOpen = false;
            onBettingClosed?.Raise();
        }

        public BetValidationResult PlaceBet(string playerId, string marbleId, int amount, CoinWallet wallet)
        {
            var result = _validator.Validate(amount, wallet, marbleId, _isBettingOpen);

            if (result != BetValidationResult.Valid)
                return result;

            // Deduct coins
            wallet.Deduct(amount);

            // Create and add bet
            var bet = new Bet(playerId, marbleId, amount, System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            _currentPool.AddBet(bet);

            onBetPlaced?.Raise();
            return BetValidationResult.Valid;
        }

        public Dictionary<string, float> GetCurrentOdds(List<string> allMarbleIds)
        {
            // Ensure all marbles have entries
            var betsPerMarble = _currentPool.GetBetsPerMarble();
            foreach (var id in allMarbleIds)
            {
                if (!betsPerMarble.ContainsKey(id))
                    betsPerMarble[id] = 0;
            }

            return OddsCalculator.CalculateOdds(betsPerMarble, _currentPool.TotalPool);
        }

        public List<PayoutResult> ResolveBets(string winningMarbleId)
        {
            var results = PayoutResolver.ResolveBets(_currentPool, winningMarbleId);

            foreach (var result in results)
            {
                if (result.Won && result.Payout > 0)
                {
                    onPayoutReceived?.Raise(result.Payout);
                }
            }

            return results;
        }
    }
}
