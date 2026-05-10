using System.Collections.Generic;
using System.Linq;

namespace MarbleRace.Core.Economy
{
    public static class OddsCalculator
    {
        private const float MinOdds = 1.1f;
        private const float MaxOdds = 20f;
        private const float HouseEdge = 0.05f; // 5% house edge

        /// <summary>
        /// Calculate pari-mutuel odds for each marble based on betting pool distribution.
        /// Returns a dictionary of marbleId → payout multiplier.
        /// </summary>
        public static Dictionary<string, float> CalculateOdds(Dictionary<string, int> betsPerMarble, int totalPool)
        {
            var odds = new Dictionary<string, float>();

            if (totalPool <= 0)
            {
                // No bets placed yet — return even odds for all
                foreach (var marble in betsPerMarble.Keys)
                    odds[marble] = (float)betsPerMarble.Count;
                return odds;
            }

            float adjustedPool = totalPool * (1f - HouseEdge);

            foreach (var kvp in betsPerMarble)
            {
                if (kvp.Value <= 0)
                {
                    odds[kvp.Key] = MaxOdds;
                    continue;
                }

                float rawOdds = adjustedPool / kvp.Value;
                odds[kvp.Key] = Clamp(rawOdds, MinOdds, MaxOdds);
            }

            return odds;
        }

        /// <summary>
        /// Calculate odds for a single marble given its bet amount and total pool.
        /// </summary>
        public static float CalculateSingleOdds(int betOnMarble, int totalPool)
        {
            if (totalPool <= 0 || betOnMarble <= 0)
                return MaxOdds;

            float adjustedPool = totalPool * (1f - HouseEdge);
            float rawOdds = adjustedPool / betOnMarble;
            return Clamp(rawOdds, MinOdds, MaxOdds);
        }

        /// <summary>
        /// Get the percentage of bets placed on each marble.
        /// </summary>
        public static Dictionary<string, float> GetBetDistribution(Dictionary<string, int> betsPerMarble, int totalPool)
        {
            var distribution = new Dictionary<string, float>();

            if (totalPool <= 0)
            {
                foreach (var marble in betsPerMarble.Keys)
                    distribution[marble] = 0f;
                return distribution;
            }

            foreach (var kvp in betsPerMarble)
            {
                distribution[kvp.Key] = (float)kvp.Value / totalPool * 100f;
            }

            return distribution;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
