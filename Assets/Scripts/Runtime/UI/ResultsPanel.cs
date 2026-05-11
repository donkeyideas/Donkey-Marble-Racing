using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MarbleRace.Core.Economy;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.UI
{
    public class ResultsPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private BettingManager bettingManager;
        [SerializeField] private EconomyManager economyManager;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private TMP_Text payoutText;
        [SerializeField] private TMP_Text resultMessage;
        [SerializeField] private TMP_Text raceTimeText;
        [SerializeField] private TMP_Text newBalanceText;
        [SerializeField] private Transform finishOrderContainer;
        [SerializeField] private GameObject finishOrderEntryPrefab;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private GameObject winEffect;
        [SerializeField] private GameObject loseEffect;

        private void OnEnable()
        {
            DisplayResults();

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgain);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void OnDisable()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.RemoveListener(OnPlayAgain);
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenu);
        }

        private void DisplayResults()
        {
            var result = raceManager.LastResult;
            if (result == null) return;

            // Resolve bets
            var payouts = bettingManager.ResolveBets(result.WinnerMarbleId);

            // Find winner marble color and name
            Color winnerColor = Color.white;
            string winnerName = "";
            foreach (var marble in raceManager.ActiveMarbles)
            {
                var identity = marble.GetComponent<MarbleIdentity>();
                if (identity != null && identity.MarbleId == result.WinnerMarbleId)
                {
                    winnerColor = identity.MarbleColor;
                    winnerName = identity.MarbleName;
                    break;
                }
            }

            if (winnerText != null)
            {
                string hex = ColorUtility.ToHtmlStringRGB(winnerColor);
                winnerText.text = $"Winner: <color=#{hex}>\u25CF {winnerName}</color>";
            }

            if (raceTimeText != null)
                raceTimeText.text = result.WasTimeout ? "TIME OUT" : $"Time: {result.RaceDuration:F2}s";

            // Check player payout
            PayoutResult? playerPayout = null;
            foreach (var p in payouts)
            {
                if (p.PlayerId == "local_player")
                {
                    playerPayout = p;
                    break;
                }
            }

            if (playerPayout.HasValue)
            {
                var pp = playerPayout.Value;
                if (pp.Won)
                {
                    economyManager.AddCoins(pp.Payout);
                    if (payoutText != null)
                        payoutText.text = $"+{pp.Payout} coins! ({pp.Odds:F1}x)";
                    if (resultMessage != null)
                        resultMessage.text = "YOU WON!";
                    if (winEffect != null) winEffect.SetActive(true);
                    if (loseEffect != null) loseEffect.SetActive(false);
                    AudioManager.Instance?.PlayWinFanfare();
                    AudioManager.Instance?.PlayCoinReward();
                }
                else
                {
                    if (payoutText != null)
                        payoutText.text = $"-{pp.BetAmount} coins";
                    if (resultMessage != null)
                        resultMessage.text = "Better luck next time!";
                    if (winEffect != null) winEffect.SetActive(false);
                    if (loseEffect != null) loseEffect.SetActive(true);
                    AudioManager.Instance?.PlayLoseBuzz();
                }
            }

            if (newBalanceText != null)
                newBalanceText.text = $"Balance: {economyManager.Balance}";

            // Check bailout
            if (economyManager.Balance <= 0)
            {
                economyManager.TryBailout();
                if (newBalanceText != null)
                    newBalanceText.text = $"Balance: {economyManager.Balance} (bailout!)";
            }

            PopulateFinishOrder(result.FinishOrder);
        }

        private void PopulateFinishOrder(List<string> order)
        {
            if (finishOrderContainer == null || finishOrderEntryPrefab == null) return;

            foreach (Transform child in finishOrderContainer)
                Destroy(child.gameObject);

            for (int i = 0; i < order.Count; i++)
            {
                var entry = Instantiate(finishOrderEntryPrefab, finishOrderContainer);
                var text = entry.GetComponentInChildren<TMP_Text>();

                // Find marble color and name
                Color marbleColor = Color.white;
                string marbleName = "";
                foreach (var marble in raceManager.ActiveMarbles)
                {
                    var identity = marble.GetComponent<MarbleIdentity>();
                    if (identity != null && identity.MarbleId == order[i])
                    {
                        marbleColor = identity.MarbleColor;
                        marbleName = identity.MarbleName;
                        break;
                    }
                }

                if (text != null)
                {
                    string hex = ColorUtility.ToHtmlStringRGB(marbleColor);
                    text.text = $"{i + 1}. <color=#{hex}>\u25CF {marbleName}</color>";
                }
            }
        }

        private void OnPlayAgain()
        {
            AudioManager.Instance?.PlayButtonTap();
            GameManager.Instance.RequestNewRace();
        }

        private void OnMainMenu()
        {
            AudioManager.Instance?.PlayButtonTap();
            GameManager.Instance.ReturnToMenu();
        }
    }
}
