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

            var sb = new System.Text.StringBuilder();
            int position = 1;

            // First: show finished marbles in locked finish order
            var finishOrder = raceManager.FinishOrder;
            foreach (var marbleId in finishOrder)
            {
                if (position > 5) break;
                foreach (var marble in marbles)
                {
                    var identity = marble.GetComponent<MarbleIdentity>();
                    if (identity != null && identity.MarbleId == marbleId)
                    {
                        string hex = ColorUtility.ToHtmlStringRGB(identity.MarbleColor);
                        sb.AppendLine($"{position}. <color=#{hex}>\u25cf</color> \u2713");
                        position++;
                        break;
                    }
                }
            }

            // Then: show unfinished marbles sorted by z position
            if (position <= 5)
            {
                var racing = new List<MarbleController>();
                foreach (var marble in marbles)
                {
                    if (marble != null && !marble.HasFinished)
                        racing.Add(marble);
                }
                racing.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));

                for (int i = 0; i < racing.Count && position <= 5; i++)
                {
                    var identity = racing[i].GetComponent<MarbleIdentity>();
                    if (identity != null)
                    {
                        string hex = ColorUtility.ToHtmlStringRGB(identity.MarbleColor);
                        sb.AppendLine($"{position}. <color=#{hex}>\u25cf</color>");
                    }
                    position++;
                }
            }

            positionsText.text = sb.ToString();
        }
    }
}
