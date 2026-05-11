using MarbleRace.Core.Economy;

namespace MarbleRace.Core.Betting
{
    public enum BetValidationResult
    {
        Valid,
        InsufficientFunds,
        BelowMinimum,
        AboveMaximum,
        InvalidAmount,
        InvalidMarble,
        BettingClosed
    }

    public class BetValidator
    {
        private readonly int _minBet;
        private readonly int _maxBet;

        public BetValidator(int minBet, int maxBet)
        {
            _minBet = minBet;
            _maxBet = maxBet;
        }

        public BetValidationResult Validate(int amount, CoinWallet wallet, string marbleId, bool bettingOpen)
        {
            if (!bettingOpen)
                return BetValidationResult.BettingClosed;

            if (string.IsNullOrEmpty(marbleId))
                return BetValidationResult.InvalidMarble;

            if (amount <= 0)
                return BetValidationResult.InvalidAmount;

            if (amount < _minBet)
                return BetValidationResult.BelowMinimum;

            if (amount > _maxBet)
                return BetValidationResult.AboveMaximum;

            if (wallet == null || !wallet.CanAfford(amount))
                return BetValidationResult.InsufficientFunds;

            return BetValidationResult.Valid;
        }
    }
}
