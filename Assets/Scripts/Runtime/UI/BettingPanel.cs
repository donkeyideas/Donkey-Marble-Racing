using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Marble;
using MarbleRace.Core.Betting;
using MarbleRace.Data;

namespace MarbleRace.Runtime.UI
{
    public class BettingPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BettingManager bettingManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private RaceManager raceManager;

        [Header("UI Elements")]
        [SerializeField] private Transform marbleButtonContainer;
        [SerializeField] private GameObject marbleButtonPrefab;
        [SerializeField] private TMP_Text selectedMarbleText;
        [SerializeField] private TMP_Text oddsText;
        [SerializeField] private TMP_Text betAmountText;
        [SerializeField] private Slider betSlider;
        [SerializeField] private Button[] quickBetButtons;
        [SerializeField] private Button confirmBetButton;
        [SerializeField] private Button skipBetButton;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private RaceHUD raceHUD;

        private string _selectedMarbleId;
        private int _currentBetAmount;
        private List<MarbleController> _marbles;

        private void OnEnable()
        {
            _selectedMarbleId = null;
            _currentBetAmount = 0;
            UpdateUI();
            PopulateMarbleButtons();

            if (betSlider != null)
                betSlider.onValueChanged.AddListener(OnBetSliderChanged);

            if (confirmBetButton != null)
                confirmBetButton.onClick.AddListener(OnConfirmBet);

            if (skipBetButton != null)
                skipBetButton.onClick.AddListener(OnSkipBet);

            SetupQuickBetButtons();
        }

        private void OnDisable()
        {
            if (betSlider != null)
                betSlider.onValueChanged.RemoveListener(OnBetSliderChanged);

            if (confirmBetButton != null)
                confirmBetButton.onClick.RemoveListener(OnConfirmBet);

            if (skipBetButton != null)
                skipBetButton.onClick.RemoveListener(OnSkipBet);
        }

        private void PopulateMarbleButtons()
        {
            // Clear existing
            foreach (Transform child in marbleButtonContainer)
                Destroy(child.gameObject);

            _marbles = raceManager.ActiveMarbles;

            foreach (var marble in _marbles)
            {
                var identity = marble.GetComponent<MarbleIdentity>();
                if (identity == null) continue;

                var buttonObj = Instantiate(marbleButtonPrefab, marbleButtonContainer);
                var button = buttonObj.GetComponent<Button>();
                var text = buttonObj.GetComponentInChildren<TMP_Text>();
                var image = buttonObj.GetComponent<Image>();

                // Just show color, no text
                if (text != null) text.text = "";
                if (image != null) image.color = identity.MarbleColor;

                string marbleId = identity.MarbleId;
                button.onClick.AddListener(() => SelectMarble(marbleId));
            }
        }

        private void SelectMarble(string marbleId)
        {
            _selectedMarbleId = marbleId;
            AudioManager.Instance?.PlayButtonTap();
            UpdateUI();
        }

        private void OnBetSliderChanged(float value)
        {
            int balance = economyManager.Balance;
            _currentBetAmount = Mathf.RoundToInt(value * balance);
            _currentBetAmount = Mathf.Max(_currentBetAmount, 10); // Enforce minimum
            UpdateUI();
        }

        private void SetupQuickBetButtons()
        {
            if (quickBetButtons == null) return;

            for (int i = 0; i < quickBetButtons.Length; i++)
            {
                int index = i;
                quickBetButtons[i].onClick.AddListener(() =>
                {
                    _currentBetAmount = economyManager.GetQuickBetAmount(index);
                    if (betSlider != null)
                        betSlider.value = (float)_currentBetAmount / economyManager.Balance;
                    UpdateUI();
                });
            }
        }

        private void OnConfirmBet()
        {
            if (string.IsNullOrEmpty(_selectedMarbleId) || _currentBetAmount <= 0) return;

            var result = bettingManager.PlaceBet(
                "local_player",
                _selectedMarbleId,
                _currentBetAmount,
                economyManager.Wallet
            );

            if (result == BetValidationResult.Valid)
            {
                AudioManager.Instance?.PlayBetPlaced();
                confirmBetButton.interactable = false;
                if (skipBetButton != null)
                    skipBetButton.interactable = false;

                // Tell HUD about the player's bet (color dot)
                if (raceHUD != null)
                {
                    string betMarbleColor = "FFFFFF";
                    foreach (var m in _marbles)
                    {
                        var id = m.GetComponent<MarbleIdentity>();
                        if (id != null && id.MarbleId == _selectedMarbleId)
                        {
                            betMarbleColor = ColorUtility.ToHtmlStringRGB(id.MarbleColor);
                            break;
                        }
                    }
                    raceHUD.SetPlayerBetColor(betMarbleColor, _currentBetAmount);
                }

                // Trigger race start after bet
                GameManager.Instance.OnBettingComplete();
            }
            else
            {
                Debug.LogWarning($"Bet invalid: {result}");
            }
        }

        private void OnSkipBet()
        {
            // Skip betting and just watch the race
            GameManager.Instance.OnBettingComplete();
        }

        private void UpdateUI()
        {
            if (balanceText != null)
                balanceText.text = $"{economyManager.Balance} coins";

            if (selectedMarbleText != null)
            {
                if (string.IsNullOrEmpty(_selectedMarbleId) || _marbles == null)
                {
                    selectedMarbleText.text = "Select a marble";
                }
                else
                {
                    // Show colored dot for the selected marble
                    foreach (var m in _marbles)
                    {
                        var id = m.GetComponent<MarbleIdentity>();
                        if (id != null && id.MarbleId == _selectedMarbleId)
                        {
                            string hex = ColorUtility.ToHtmlStringRGB(id.MarbleColor);
                            selectedMarbleText.text = $"<color=#{hex}>\u25CF</color> Selected";
                            break;
                        }
                    }
                }
            }

            if (betAmountText != null)
                betAmountText.text = $"Bet: {_currentBetAmount}";

            if (confirmBetButton != null)
                confirmBetButton.interactable = !string.IsNullOrEmpty(_selectedMarbleId) && _currentBetAmount > 0;

            UpdateOddsDisplay();
        }

        private void UpdateOddsDisplay()
        {
            if (oddsText == null || string.IsNullOrEmpty(_selectedMarbleId)) return;

            var marbleIds = new List<string>();
            foreach (var m in _marbles)
                marbleIds.Add(m.MarbleId);

            var odds = bettingManager.GetCurrentOdds(marbleIds);

            if (odds.ContainsKey(_selectedMarbleId))
                oddsText.text = $"Odds: {odds[_selectedMarbleId]:F1}x";
        }
    }
}
