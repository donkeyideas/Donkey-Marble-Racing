using System.Collections.Generic;
using MarbleRace.Core.Betting;

namespace MarbleRace.Core.Economy
{
    public struct PayoutResult
    {
        public string PlayerId;
        public string MarbleId;
        public int BetAmount;
        public float Odds;
        public int Payout;
        public bool Won;
    }

    public static class PayoutResolver
    {
        /// <summary>
        /// Resolve all bets for a completed race.
        /// Returns payout results for every bet placed.
        /// </summary>
        public static List<PayoutResult> ResolveBets(BettingPool pool, string winningMarbleId)
        {
            var results = new List<PayoutResult>();
            var odds = OddsCalculator.CalculateOdds(pool.GetBetsPerMarble(), pool.TotalPool);

            foreach (var bet in pool.AllBets)
            {
                bool won = bet.MarbleId == winningMarbleId;
                float betOdds = odds.ContainsKey(bet.MarbleId) ? odds[bet.MarbleId] : 1f;
                int payout = won ? (int)(bet.Amount * betOdds) : 0;

                results.Add(new PayoutResult
                {
                    PlayerId = bet.PlayerId,
                    MarbleId = bet.MarbleId,
                    BetAmount = bet.Amount,
                    Odds = betOdds,
                    Payout = payout,
                    Won = won
                });
            }

            return results;
        }

        /// <summary>
        /// Resolve a single player's bet.
        /// </summary>
        public static PayoutResult ResolveSingleBet(Bet bet, string winningMarbleId, float odds)
        {
            bool won = bet.MarbleId == winningMarbleId;
            int payout = won ? (int)(bet.Amount * odds) : 0;

            return new PayoutResult
            {
                PlayerId = bet.PlayerId,
                MarbleId = bet.MarbleId,
                BetAmount = bet.Amount,
                Odds = odds,
                Payout = payout,
                Won = won
            };
        }
    }
}
