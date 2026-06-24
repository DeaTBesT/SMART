using Controllers;
using Gameplay;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UIModule
{
    public class PlayerActionUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _createAreaButton;
        [SerializeField] private Button _upgradeAreaButton;

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
