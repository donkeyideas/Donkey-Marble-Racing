using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MarbleRace.Runtime.Managers;

namespace MarbleRace.Runtime.UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button dailyRewardButton;

        [Header("Display")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text coinBalanceText;
        [SerializeField] private TMP_Text welcomeText;

        [Header("References")]
        [SerializeField] private EconomyManager economyManager;

        private void OnEnable()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlay);
            if (dailyRewardButton != null)
                dailyRewardButton.onClick.AddListener(OnClaimDailyReward);
        }

        private void Start()
        {
            UpdateDisplay();
        }

        private void OnDisable()
        {
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlay);
            if (dailyRewardButton != null)
                dailyRewardButton.onClick.RemoveListener(OnClaimDailyReward);
        }

        private void OnPlay()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RequestNewRace();
            else
                Debug.LogError("GameManager.Instance is null!");
        }

        private void OnClaimDailyReward()
        {
            if (economyManager != null)
            {
                economyManager.ClaimDailyReward();
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (coinBalanceText != null && economyManager != null)
                coinBalanceText.text = $"{economyManager.Balance} coins";
        }
    }
}
