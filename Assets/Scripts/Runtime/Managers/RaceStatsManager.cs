using System.Collections.Generic;
using UnityEngine;

namespace MarbleRace.Runtime.Managers
{
    [System.Serializable]
    public class MarbleStats
    {
        public string marbleId;
        public int wins;
        public int totalRaces;
        public int topThreeFinishes;
        public float bestTime;

        public float WinRate => totalRaces > 0 ? (float)wins / totalRaces : 0f;
    }

    [System.Serializable]
    public class RaceStatsData
    {
        public int totalRacesPlayed;
        public int totalBetsWon;
        public int totalBetsLost;
        public int totalCoinsWon;
        public int totalCoinsLost;
        public int currentWinStreak;
        public int bestWinStreak;
        public List<MarbleStats> marbleStats = new List<MarbleStats>();
    }

    public class RaceStatsManager : MonoBehaviour
    {
        public static RaceStatsManager Instance { get; private set; }

        private RaceStatsData _stats;
        private const string SAVE_KEY = "MarbleRace_Stats";

        public RaceStatsData Stats => _stats;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadStats();
        }

        public void RecordRaceResult(string winnerMarbleId, List<string> finishOrder, float raceTime, bool betWon, int coinsChange)
        {
            _stats.totalRacesPlayed++;

            // Update marble stats
            for (int i = 0; i < finishOrder.Count; i++)
            {
                var marbleStat = GetOrCreateMarbleStat(finishOrder[i]);
                marbleStat.totalRaces++;

                if (i == 0)
                {
                    marbleStat.wins++;
                    if (raceTime > 0 && (marbleStat.bestTime <= 0 || raceTime < marbleStat.bestTime))
                        marbleStat.bestTime = raceTime;
                }
                if (i < 3)
                    marbleStat.topThreeFinishes++;
            }

            // Update betting stats
            if (betWon)
            {
                _stats.totalBetsWon++;
                _stats.totalCoinsWon += coinsChange;
                _stats.currentWinStreak++;
                if (_stats.currentWinStreak > _stats.bestWinStreak)
                    _stats.bestWinStreak = _stats.currentWinStreak;
            }
            else
            {
                _stats.totalBetsLost++;
                _stats.totalCoinsLost += Mathf.Abs(coinsChange);
                _stats.currentWinStreak = 0;
            }

            SaveStats();
        }

        public MarbleStats GetMarbleStat(string marbleId)
        {
            return _stats.marbleStats.Find(s => s.marbleId == marbleId);
        }

        private MarbleStats GetOrCreateMarbleStat(string marbleId)
        {
            var existing = _stats.marbleStats.Find(s => s.marbleId == marbleId);
            if (existing != null) return existing;

            var newStat = new MarbleStats { marbleId = marbleId };
            _stats.marbleStats.Add(newStat);
            return newStat;
        }

        public void ResetStats()
        {
            _stats = new RaceStatsData();
            SaveStats();
        }

        private void SaveStats()
        {
            string json = JsonUtility.ToJson(_stats);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadStats()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                _stats = JsonUtility.FromJson<RaceStatsData>(json);
            }
            else
            {
                _stats = new RaceStatsData();
            }
        }
    }
}
