using UnityEngine;

namespace MarbleRace.Data
{
    [CreateAssetMenu(menuName = "MarbleRace/Data/Economy Config")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Starting")]
        public int startingCoins = 1000;

        [Header("Betting Limits")]
        public int minimumBet = 10;
        public int maximumBet = 500;

        [Header("Rewards")]
        public int dailyLoginReward = 200;
        public int bailoutAmount = 100;
        public float bailoutCooldownHours = 1f;

        [Header("Quick Bet Percentages")]
        [Range(0f, 1f)] public float quickBet1 = 0.25f;
        [Range(0f, 1f)] public float quickBet2 = 0.50f;
        [Range(0f, 1f)] public float quickBet3 = 1.00f;
    }
}
