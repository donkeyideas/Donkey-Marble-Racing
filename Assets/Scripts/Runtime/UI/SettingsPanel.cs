using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MarbleRace.Runtime.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle particlesToggle;
        [SerializeField] private Slider qualitySlider;
        [SerializeField] private TextMeshProUGUI qualityLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetStatsButton;

        private const string PREF_SOUND = "Settings_Sound";
        private const string PREF_PARTICLES = "Settings_Particles";
        private const string PREF_QUALITY = "Settings_Quality";

        public static bool SoundEnabled { get; private set; } = true;
        public static bool ParticlesEnabled { get; private set; } = true;
        public static int QualityLevel { get; private set; } = 2;

        private void Awake()
        {
            LoadSettings();

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            if (resetStatsButton != null)
                resetStatsButton.onClick.AddListener(ResetStats);
            if (soundToggle != null)
            {
                soundToggle.isOn = SoundEnabled;
                soundToggle.onValueChanged.AddListener(OnSoundToggled);
            }
            if (particlesToggle != null)
            {
                particlesToggle.isOn = ParticlesEnabled;
                particlesToggle.onValueChanged.AddListener(OnParticlesToggled);
            }
            if (qualitySlider != null)
            {
                qualitySlider.minValue = 0;
                qualitySlider.maxValue = 2;
                qualitySlider.wholeNumbers = true;
                qualitySlider.value = QualityLevel;
                qualitySlider.onValueChanged.AddListener(OnQualityChanged);
            }

            UpdateQualityLabel();
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnSoundToggled(bool value)
        {
            SoundEnabled = value;
            AudioListener.volume = value ? 1f : 0f;
            PlayerPrefs.SetInt(PREF_SOUND, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnParticlesToggled(bool value)
        {
            ParticlesEnabled = value;
            PlayerPrefs.SetInt(PREF_PARTICLES, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnQualityChanged(float value)
        {
            QualityLevel = (int)value;
            ApplyQuality();
            UpdateQualityLabel();
            PlayerPrefs.SetInt(PREF_QUALITY, QualityLevel);
            PlayerPrefs.Save();
        }

        private void UpdateQualityLabel()
        {
            if (qualityLabel == null) return;
            switch (QualityLevel)
            {
                case 0: qualityLabel.text = "LOW"; break;
                case 1: qualityLabel.text = "MEDIUM"; break;
                case 2: qualityLabel.text = "HIGH"; break;
            }
        }

        private void ApplyQuality()
        {
            switch (QualityLevel)
            {
                case 0: // Low — mobile battery saver
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.pixelLightCount = 1;
                    break;
                case 1: // Medium
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.pixelLightCount = 2;
                    break;
                case 2: // High
                    QualitySettings.antiAliasing = 4;
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.pixelLightCount = 4;
                    break;
            }
        }

        private void ResetStats()
        {
            if (Managers.RaceStatsManager.Instance != null)
                Managers.RaceStatsManager.Instance.ResetStats();
        }

        private void LoadSettings()
        {
            SoundEnabled = PlayerPrefs.GetInt(PREF_SOUND, 1) == 1;
            ParticlesEnabled = PlayerPrefs.GetInt(PREF_PARTICLES, 1) == 1;
            QualityLevel = PlayerPrefs.GetInt(PREF_QUALITY, 2);

            AudioListener.volume = SoundEnabled ? 1f : 0f;
            ApplyQuality();
        }
    }
}
