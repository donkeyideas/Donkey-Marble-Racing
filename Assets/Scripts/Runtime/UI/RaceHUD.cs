using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.UI
{
    public class RaceHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RaceManager raceManager;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text positionsText;
        [SerializeField] private TMP_Text playerBetIndicator;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private GameObject countdownPanel;

        private void Update()
        {
            if (raceManager == null) return;

            if (raceManager.IsRacing)
            {
                UpdateTimer();
                UpdatePositions();
            }
        }

        public void ShowCountdown(int count)
        {
            if (countdownPanel != null)
                countdownPanel.SetActive(true);

            if (countdownText != null)
            {
                countdownText.text = count > 0 ? count.ToString() : "GO!";
                // Scale animation for impact
                countdownText.transform.localScale = Vector3.one * 1.5f;
            }
        }

        public void HideCountdown()
        {
            if (countdownPanel != null)
                countdownPanel.SetActive(false);
        }

        public void SetPlayerBet(string marbleName, int amount)
        {
            if (playerBetIndicator != null)
                playerBetIndicator.text = $"Your bet: {amount} on {marbleName}";
        }

        private void UpdateTimer()
        {
            if (timerText != null)
            {
                float time = raceManager.RaceTime;
                timerText.text = $"{time:F1}s";
            }
        }

        private void UpdatePositions()
        {
            if (positionsText == null) return;

            var marbles = raceManager.ActiveMarbles;
            if (marbles == null || marbles.Count == 0) return;

            // Sort by Z position (progress)
            var sorted = new List<MarbleController>(marbles);
            sorted.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Mathf.Min(sorted.Count, 5); i++)
            {
                var identity = sorted[i].GetComponent<MarbleIdentity>();
                string name = identity != null ? identity.MarbleName : $"Marble {i + 1}";
                sb.AppendLine($"{i + 1}. {name}");
            }

            positionsText.text = sb.ToString();
        }
    }
}
