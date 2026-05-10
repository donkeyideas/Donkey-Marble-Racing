using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.UI
{
    public class OddsDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BettingManager bettingManager;
        [SerializeField] private RaceManager raceManager;

        [Header("UI")]
        [SerializeField] private Transform oddsContainer;
        [SerializeField] private GameObject oddsEntryPrefab;
        [SerializeField] private float updateInterval = 0.5f;

        private float _lastUpdateTime;
        private List<TMP_Text> _oddsTexts = new List<TMP_Text>();

        private void OnEnable()
        {
            RefreshOddsDisplay();
        }

        private void Update()
        {
            if (!bettingManager.IsBettingOpen) return;

            if (Time.time - _lastUpdateTime > updateInterval)
            {
                RefreshOddsDisplay();
                _lastUpdateTime = Time.time;
            }
        }

        private void RefreshOddsDisplay()
        {
            if (oddsContainer == null || raceManager == null) return;

            var marbles = raceManager.ActiveMarbles;
            if (marbles == null || marbles.Count == 0) return;

            // Get all marble IDs
            var marbleIds = new List<string>();
            foreach (var marble in marbles)
                marbleIds.Add(marble.MarbleId);

            var odds = bettingManager.GetCurrentOdds(marbleIds);

            // Rebuild if count changed
            if (_oddsTexts.Count != marbles.Count)
            {
                foreach (Transform child in oddsContainer)
                    Destroy(child.gameObject);
                _oddsTexts.Clear();

                foreach (var marble in marbles)
                {
                    var entry = Instantiate(oddsEntryPrefab, oddsContainer);
                    var text = entry.GetComponentInChildren<TMP_Text>();
                    _oddsTexts.Add(text);
                }
            }

            // Update text
            for (int i = 0; i < marbles.Count && i < _oddsTexts.Count; i++)
            {
                var identity = marbles[i].GetComponent<MarbleIdentity>();
                string name = identity != null ? identity.MarbleName : $"Marble {i + 1}";
                float marbleOdds = odds.ContainsKey(marbles[i].MarbleId) ? odds[marbles[i].MarbleId] : 8f;

                if (_oddsTexts[i] != null)
                    _oddsTexts[i].text = $"{name}: {marbleOdds:F1}x";
            }
        }
    }
}
