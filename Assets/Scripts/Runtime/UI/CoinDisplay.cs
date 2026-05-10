using UnityEngine;
using TMPro;
using MarbleRace.Runtime.Managers;

namespace MarbleRace.Runtime.UI
{
    public class CoinDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private Animator coinAnimator;

        private int _displayedAmount;

        private void Update()
        {
            if (economyManager == null || coinText == null) return;

            int current = economyManager.Balance;
            if (current != _displayedAmount)
            {
                _displayedAmount = current;
                coinText.text = FormatCoins(current);

                if (coinAnimator != null)
                    coinAnimator.SetTrigger("Pulse");
            }
        }

        public void OnCoinsChanged(int newBalance)
        {
            _displayedAmount = newBalance;
            if (coinText != null)
                coinText.text = FormatCoins(newBalance);

            if (coinAnimator != null)
                coinAnimator.SetTrigger("Pulse");
        }

        private string FormatCoins(int amount)
        {
            if (amount >= 1000000)
                return $"{amount / 1000000f:F1}M";
            if (amount >= 1000)
                return $"{amount / 1000f:F1}K";
            return amount.ToString();
        }
    }
}
