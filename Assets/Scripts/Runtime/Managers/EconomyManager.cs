using System;
using UnityEngine;
using MarbleRace.Core.Economy;
using MarbleRace.Data;
using MarbleRace.Events;

namespace MarbleRace.Runtime.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private EconomyConfig config;

        [Header("Events")]
        [SerializeField] private IntEvent onCoinsChanged;
        [SerializeField] private GameEvent onBailoutUsed;

        private CoinWallet _wallet;
        private DateTime _lastBailoutTime;

        public CoinWallet Wallet => _wallet;
        public int Balance => _wallet != null ? _wallet.Balance : 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _wallet = new CoinWallet(config.startingCoins);
            _lastBailoutTime = DateTime.MinValue;

            LoadSavedBalance();
        }

        public void AddCoins(int amount)
        {
            _wallet.Add(amount);
            onCoinsChanged?.Raise(_wallet.Balance);
            SaveBalance();
        }

        public bool SpendCoins(int amount)
        {
            if (!_wallet.Deduct(amount))
                return false;

            onCoinsChanged?.Raise(_wallet.Balance);
            SaveBalance();
            return true;
        }

        public bool TryBailout()
        {
            if (_wallet.Balance > 0) return false;

            var timeSinceLastBailout = DateTime.UtcNow - _lastBailoutTime;
            if (timeSinceLastBailout.TotalHours < config.bailoutCooldownHours)
                return false;

            _wallet.Add(config.bailoutAmount);
            _lastBailoutTime = DateTime.UtcNow;
            onCoinsChanged?.Raise(_wallet.Balance);
            onBailoutUsed?.Raise();
            SaveBalance();
            return true;
        }

        public void ClaimDailyReward()
        {
            string lastClaimKey = "LastDailyReward";
            string lastClaim = PlayerPrefs.GetString(lastClaimKey, "");
            string today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

            if (lastClaim == today) return;

            _wallet.Add(config.dailyLoginReward);
            PlayerPrefs.SetString(lastClaimKey, today);
            onCoinsChanged?.Raise(_wallet.Balance);
            SaveBalance();
        }

        public int GetQuickBetAmount(int index)
        {
            float percentage = index switch
            {
                0 => config.quickBet1,
                1 => config.quickBet2,
                2 => config.quickBet3,
                _ => config.quickBet1
            };

            int amount = Mathf.FloorToInt(_wallet.Balance * percentage);
            return Mathf.Clamp(amount, config.minimumBet, config.maximumBet);
        }

        private void SaveBalance()
        {
            PlayerPrefs.SetInt("CoinBalance", _wallet.Balance);
            PlayerPrefs.Save();
        }

        private void LoadSavedBalance()
        {
            if (PlayerPrefs.HasKey("CoinBalance"))
            {
                int saved = PlayerPrefs.GetInt("CoinBalance");
                _wallet.SetBalance(saved);
            }
        }
    }
}
