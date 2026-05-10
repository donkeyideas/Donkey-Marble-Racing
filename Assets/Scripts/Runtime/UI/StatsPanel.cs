using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MarbleRace.Runtime.Managers;

namespace MarbleRace.Runtime.UI
{
    public class StatsPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI marbleStatsText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshStats();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RefreshStats()
        {
            if (RaceStatsManager.Instance == null) return;

            var stats = RaceStatsManager.Instance.Stats;

            // General stats
            string general = $"<b>RACE STATISTICS</b>\n\n" +
                $"Total Races: <color=#FFD700>{stats.totalRacesPlayed}</color>\n" +
                $"Bets Won: <color=#00FF88>{stats.totalBetsWon}</color>\n" +
                $"Bets Lost: <color=#FF4444>{stats.totalBetsLost}</color>\n" +
                $"Win Rate: <color=#88CCFF>{(stats.totalBetsWon + stats.totalBetsLost > 0 ? (float)stats.totalBetsWon / (stats.totalBetsWon + stats.totalBetsLost) * 100f : 0f):F1}%</color>\n\n" +
                $"Coins Won: <color=#00FF88>+{stats.totalCoinsWon}</color>\n" +
                $"Coins Lost: <color=#FF4444>-{stats.totalCoinsLost}</color>\n" +
                $"Net Profit: <color={(stats.totalCoinsWon - stats.totalCoinsLost >= 0 ? "#00FF88" : "#FF4444")}>{stats.totalCoinsWon - stats.totalCoinsLost}</color>\n\n" +
                $"Win Streak: <color=#FFD700>{stats.currentWinStreak}</color>\n" +
                $"Best Streak: <color=#FFD700>{stats.bestWinStreak}</color>";

            if (statsText != null)
                statsText.text = general;

            // Marble performance
            if (marbleStatsText != null && stats.marbleStats.Count > 0)
            {
                // Sort by wins descending
                var sorted = new System.Collections.Generic.List<MarbleStats>(stats.marbleStats);
                sorted.Sort((a, b) => b.wins.CompareTo(a.wins));

                string marbleInfo = "<b>MARBLE RANKINGS</b>\n\n";
                int rank = 1;
                foreach (var m in sorted)
                {
                    string winRate = m.totalRaces > 0 ? $"{m.WinRate * 100f:F0}%" : "N/A";
                    marbleInfo += $"{rank}. {m.marbleId} — {m.wins}W / {m.totalRaces}R ({winRate})\n";
                    rank++;
                }
                marbleStatsText.text = marbleInfo;
            }
        }
    }
}
