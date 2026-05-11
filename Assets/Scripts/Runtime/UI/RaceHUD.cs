using System.Collections;
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
        [SerializeField] private TMP_Text trackNameText;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private GameObject countdownPanel;

        private void OnEnable()
        {
            if (trackNameText != null && raceManager != null)
                trackNameText.text = raceManager.CurrentTrackName;
        }

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
                StopAllCoroutines();
                StartCoroutine(PunchScale(countdownText.transform));
            }
        }

        private IEnumerator PunchScale(Transform target)
        {
            float duration = 0.4f;
            float elapsed = 0f;
            Vector3 start = Vector3.one * 1.8f;
            Vector3 end = Vector3.one;

            target.localScale = start;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                // Elastic ease-out for a bouncy punch feel
                float ease = 1f - Mathf.Cos(t * Mathf.PI * 2f) * Mathf.Pow(1f - t, 2f);
                target.localScale = Vector3.LerpUnclamped(start, end, ease);
                yield return null;
            }
            target.localScale = end;
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
                        sb.AppendLine($"{position}. <color=#{hex}>\u25cf {identity.MarbleName}</color>");
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
                        sb.AppendLine($"{position}. <color=#{hex}>\u25cf {identity.MarbleName}</color>");
                    }
                    position++;
                }
            }

            positionsText.text = sb.ToString();
        }
    }
}
