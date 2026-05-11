using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.UI
{
    public class LiveBetPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LiveBetManager liveBetManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private RaceManager raceManager;

        [Header("Main Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button openLiveBetButton;
        [SerializeField] private TMP_Text progressTimerText;

        [Header("Double Down View")]
        [SerializeField] private GameObject doubleDownView;
        [SerializeField] private TMP_Text doubleDownCostText;
        [SerializeField] private TMP_Text doubleDownWinText;
        [SerializeField] private TMP_Text feePercentText;
        [SerializeField] private Button doubleDownButton;
        [SerializeField] private Button cancelButton;

        [Header("Switch View")]
        [SerializeField] private GameObject switchView;
        [SerializeField] private Transform switchMarbleContainer;
        [SerializeField] private GameObject switchMarbleButtonPrefab;
        [SerializeField] private TMP_Text switchInfoText;
        [SerializeField] private Button showSwitchButton;
        [SerializeField] private Button backToMainButton;

        private bool _showingDetails;
        private bool _showingSwitch;
        private int _doubleDownAmount;

        private void OnEnable()
        {
            if (openLiveBetButton != null)
                openLiveBetButton.onClick.AddListener(OnOpenLiveBet);
            if (doubleDownButton != null)
                doubleDownButton.onClick.AddListener(OnDoubleDown);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancel);
            if (showSwitchButton != null)
                showSwitchButton.onClick.AddListener(OnShowSwitch);
            if (backToMainButton != null)
                backToMainButton.onClick.AddListener(OnBackToMain);

            _showingDetails = false;
            _showingSwitch = false;
            HideDetails();
        }

        private void OnDisable()
        {
            if (openLiveBetButton != null)
                openLiveBetButton.onClick.RemoveListener(OnOpenLiveBet);
            if (doubleDownButton != null)
                doubleDownButton.onClick.RemoveListener(OnDoubleDown);
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancel);
            if (showSwitchButton != null)
                showSwitchButton.onClick.RemoveListener(OnShowSwitch);
            if (backToMainButton != null)
                backToMainButton.onClick.RemoveListener(OnBackToMain);
        }

        private void Update()
        {
            if (liveBetManager == null) return;

            // Show/hide the live bet button based on window state
            bool windowOpen = liveBetManager.IsWindowOpen;

            if (panelRoot != null)
                panelRoot.SetActive(windowOpen);

            if (!windowOpen)
            {
                HideDetails();
                return;
            }

            // Update progress timer
            if (progressTimerText != null)
            {
                float progress = liveBetManager.RaceProgress;
                float remaining = Mathf.Clamp01(0.5f - progress) / 0.5f; // percent of window remaining
                progressTimerText.text = $"{Mathf.RoundToInt(remaining * 100)}%";
            }

            // Update cost display if details are showing
            if (_showingDetails && !_showingSwitch)
                UpdateDoubleDownDisplay();
        }

        private void OnOpenLiveBet()
        {
            _showingDetails = true;
            _showingSwitch = false;

            // Default double-down amount = 50% of current balance (capped at original bet)
            _doubleDownAmount = Mathf.Min(
                Mathf.RoundToInt(economyManager.Balance * 0.5f),
                liveBetManager.OriginalBetAmount
            );
            _doubleDownAmount = Mathf.Max(_doubleDownAmount, 10);

            ShowDoubleDown();
            AudioManager.Instance?.PlayButtonTap();
        }

        private void ShowDoubleDown()
        {
            if (doubleDownView != null) doubleDownView.SetActive(true);
            if (switchView != null) switchView.SetActive(false);
            UpdateDoubleDownDisplay();
        }

        private void UpdateDoubleDownDisplay()
        {
            liveBetManager.GetDoubleDownCost(_doubleDownAmount, out int totalCost, out int fee);
            int potentialWin = liveBetManager.GetDoubleDownPotentialWin(_doubleDownAmount);
            float feePercent = liveBetManager.CurrentFeePercent;

            if (doubleDownCostText != null)
                doubleDownCostText.text = $"Cost: {totalCost} ({_doubleDownAmount} + {fee} fee)";
            if (doubleDownWinText != null)
                doubleDownWinText.text = $"Potential win: {potentialWin}";
            if (feePercentText != null)
                feePercentText.text = $"Fee: {Mathf.RoundToInt(feePercent * 100)}%";

            if (doubleDownButton != null)
                doubleDownButton.interactable = economyManager.Balance >= totalCost;
        }

        private void OnDoubleDown()
        {
            var result = liveBetManager.DoubleDown(_doubleDownAmount);
            if (result == LiveBetResult.Success)
            {
                HideDetails();
            }
            else
            {
                Debug.LogWarning($"Live bet failed: {result}");
            }
        }

        private void OnShowSwitch()
        {
            _showingSwitch = true;
            if (doubleDownView != null) doubleDownView.SetActive(false);
            if (switchView != null) switchView.SetActive(true);

            PopulateSwitchButtons();

            if (switchInfoText != null)
            {
                int forfeit = Mathf.RoundToInt(liveBetManager.OriginalBetAmount * 0.5f);
                switchInfoText.text = $"Forfeit {forfeit} coins. Bet remainder on new marble.";
            }
        }

        private void PopulateSwitchButtons()
        {
            if (switchMarbleContainer == null || switchMarbleButtonPrefab == null) return;

            foreach (Transform child in switchMarbleContainer)
                Destroy(child.gameObject);

            var marbles = raceManager.ActiveMarbles;
            var liveOdds = liveBetManager.GetLiveOdds();

            foreach (var marble in marbles)
            {
                var identity = marble.GetComponent<MarbleIdentity>();
                if (identity == null) continue;
                // Skip the marble they already bet on
                if (identity.MarbleId == liveBetManager.OriginalMarbleId) continue;

                var buttonObj = Instantiate(switchMarbleButtonPrefab, switchMarbleContainer);
                var button = buttonObj.GetComponent<Button>();
                var image = buttonObj.GetComponent<Image>();
                var text = buttonObj.GetComponentInChildren<TMP_Text>();

                if (image != null) image.color = identity.MarbleColor;

                // Show live odds
                float odds = liveOdds.ContainsKey(identity.MarbleId) ? liveOdds[identity.MarbleId] : 4f;
                int potentialWin = liveBetManager.GetSwitchPotentialWin(identity.MarbleId);
                if (text != null) text.text = $"{odds:F1}x";

                string marbleId = identity.MarbleId;
                button.onClick.AddListener(() => OnSwitchToMarble(marbleId));
            }
        }

        private void OnSwitchToMarble(string marbleId)
        {
            var result = liveBetManager.SwitchBet(marbleId);
            if (result == LiveBetResult.Success)
            {
                HideDetails();
            }
        }

        private void OnBackToMain()
        {
            _showingSwitch = false;
            ShowDoubleDown();
        }

        private void OnCancel()
        {
            HideDetails();
        }

        private void HideDetails()
        {
            _showingDetails = false;
            _showingSwitch = false;
            if (doubleDownView != null) doubleDownView.SetActive(false);
            if (switchView != null) switchView.SetActive(false);
        }
    }
}
