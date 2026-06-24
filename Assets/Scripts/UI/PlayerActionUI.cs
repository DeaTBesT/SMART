using Controllers;
using Gameplay;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIModule
{
    public class PlayerActionUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _createAreaButton;
        [SerializeField] private Button _upgradeAreaButton;
        [SerializeField] private TextMeshProUGUI _scoreText;

        private Controller _currentScoreController;

        private void Awake()
        {
            if (_createAreaButton != null)
            {
                _createAreaButton.onClick.AddListener(OnCreateAreaButtonClicked);
            }

            if (_upgradeAreaButton != null)
            {
                _upgradeAreaButton.onClick.AddListener(OnUpgradeAreaButtonClicked);
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerTurnChanged += OnPlayerTurnChanged;
            }

            if (AreaSelectionManager.Instance != null)
            {
                AreaSelectionManager.Instance.OnSelectedAreaChanged += OnSelectedAreaChanged;
            }
            
            Controller.OnScoreChanged += UpdateScoreText;

            UpdatePanelState();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerTurnChanged -= OnPlayerTurnChanged;
            }

            if (AreaSelectionManager.Instance != null)
            {
                AreaSelectionManager.Instance.OnSelectedAreaChanged -= OnSelectedAreaChanged;
            }

            Controller.OnScoreChanged -= UpdateScoreText;
        }

        private void OnPlayerTurnChanged(bool isPlayerTurn)
        {
            UpdatePanelState();
        }

        private void OnSelectedAreaChanged(Area area)
        {
            _upgradeAreaButton.gameObject.SetActive(area != null);
            _createAreaButton.gameObject.SetActive(area == null);
            UpdatePanelState();
        }

        private void UpdateScoreText()
        {
            if (_scoreText == null || GameManager.Instance == null)
            {
                return;
            }

            var playerScore = 0;
            var enemyScore = 0;

            foreach (var controller in GameManager.Instance.PlayerControllers)
            {
                if (controller is PlayerController)
                {
                    playerScore = controller.Score;
                }
                else if (controller is EnemyController)
                {
                    enemyScore = controller.Score;
                }
            }

            _scoreText.text = $"<color=#00A2FF>{playerScore}</color> : <color=#FF0000>{enemyScore}</color>";
        }

        private void UpdatePanelState()
        {
            var manager = GameManager.Instance;
            var isPlayerTurn = manager != null && manager.CurrentPlayer is PlayerController;

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(isPlayerTurn);
            }

            if (_createAreaButton != null)
            {
                _createAreaButton.interactable = isPlayerTurn && manager != null && manager.CanCreateAreaForCurrentPlayer();
            }

            if (_upgradeAreaButton != null)
            {
                _upgradeAreaButton.interactable = isPlayerTurn && manager != null && manager.CanUpgradeSelectedArea();
            }

            UpdateScoreText();
        }

        public void OnCreateAreaButtonClicked()
        {
            GameManager.Instance?.OnCreateAreaButtonPressed();
            UpdatePanelState();
        }

        public void OnUpgradeAreaButtonClicked()
        {
            GameManager.Instance?.OnUpgradeAreaButtonPressed();
            UpdatePanelState();
        }
    }
}
