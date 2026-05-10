using UnityEngine;

namespace MarbleRace.Runtime.Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject bettingPanel;
        [SerializeField] private GameObject raceHUD;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject countdownOverlay;

        [Header("References")]
        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            HideAll();
        }

        private void Start()
        {
            ShowMainMenu();
        }

        public void OnGameStateChanged()
        {
            HideAll();

            switch (gameManager.CurrentState)
            {
                case GameState.MainMenu:
                    ShowMainMenu();
                    break;
                case GameState.Betting:
                    ShowBetting();
                    break;
                case GameState.Countdown:
                    ShowCountdown();
                    break;
                case GameState.Racing:
                    ShowRaceHUD();
                    break;
                case GameState.PhotoFinish:
                    ShowRaceHUD();
                    break;
                case GameState.Results:
                    ShowResults();
                    break;
            }
        }

        private void ShowMainMenu()
        {
            SetActive(mainMenuPanel, true);
        }

        private void ShowBetting()
        {
            SetActive(bettingPanel, true);
        }

        private void ShowCountdown()
        {
            SetActive(raceHUD, true);
            SetActive(countdownOverlay, true);
        }

        private void ShowRaceHUD()
        {
            SetActive(raceHUD, true);
        }

        private void ShowResults()
        {
            SetActive(resultsPanel, true);
        }

        private void HideAll()
        {
            SetActive(mainMenuPanel, false);
            SetActive(bettingPanel, false);
            SetActive(raceHUD, false);
            SetActive(resultsPanel, false);
            SetActive(countdownOverlay, false);
        }

        private void SetActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }
    }
}
