using Managers;
using TMPro;
using UnityEngine;

namespace UIModule
{
    public class ResourcePanel : MonoBehaviour
    {
        [SerializeField] private int _teamId = 0;
        [SerializeField] private TextMeshProUGUI _woodText;
        [SerializeField] private TextMeshProUGUI _oreText;

        public int TeamId => _teamId;

        private void Awake()
        {
            SetResourceValues(0, 0);
        }

        private void OnEnable()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnChangeResources += OnResourcesChanged;
            }
        }

        private void OnDisable()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnChangeResources -= OnResourcesChanged;
            }
        }

        private void OnResourcesChanged(int teamId, int wood, int ore)
        {
            if (teamId != _teamId)
            {
                return;
            }

            SetResourceValues(wood, ore);
        }

        public void SetResourceValues(int wood, int ore)
        {
            if (_woodText != null)
            {
                _woodText.text = $"Wood: {wood}";
            }

            if (_oreText != null)
            {
                _oreText.text = $"Ore: {ore}";
            }
        }
    }
}
