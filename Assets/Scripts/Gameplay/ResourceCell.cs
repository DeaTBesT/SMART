using UnityEngine;

namespace Gameplay
{
    public class ResourceCell : MonoBehaviour
    {
        [SerializeField] private ResourceType _resourceType = ResourceType.None;
        [SerializeField] private int _amountPerTurn;
        [SerializeField] private bool _isCaptured;
        [SerializeField] private SpriteRenderer _fallbackRenderer;

        private Vector2Int _gridPosition;
        private GameObject _iconInstance;
        private int _ownerTeamId = -1;

        public ResourceType ResourceType => _resourceType;
        public int AmountPerTurn => _amountPerTurn;
        public bool IsCaptured => _isCaptured;
        public int OwnerTeamId => _ownerTeamId;
        public bool CanCapture => _resourceType != ResourceType.None && !_isCaptured;

        public void Initialize(Vector2Int gridPosition)
        {
            _gridPosition = gridPosition;
            name = $"Tile {_gridPosition.x}x{_gridPosition.y}";
            _resourceType = ResourceType.None;
            _amountPerTurn = 0;
            _isCaptured = false;
        }

        public void SetResource(ResourceType resourceType, int amountPerTurn, GameObject iconPrefab = null)
        {
            _resourceType = resourceType;
            _amountPerTurn = amountPerTurn;
            _isCaptured = false;

            if (_resourceType == ResourceType.None)
            {
                return;
            }

            if (iconPrefab != null)
            {
                _iconInstance = Instantiate(iconPrefab, transform);
                _iconInstance.transform.localPosition = Vector3.zero;
                _iconInstance.transform.localScale = Vector3.one;
                return;
            }

            if (_fallbackRenderer == null)
            {
                _fallbackRenderer = GetComponent<SpriteRenderer>();
            }

            if (_fallbackRenderer == null)
            {
                return;
            }

            _fallbackRenderer.color = _resourceType == ResourceType.Wood
                ? new Color(0.4f, 0.8f, 0.4f)
                : new Color(0.7f, 0.6f, 0.4f);
        }

        public void Capture(int teamId)
        {
            if (_isCaptured || _resourceType == ResourceType.None)
            {
                return;
            }

            _isCaptured = true;
            _ownerTeamId = teamId;

            if (_iconInstance != null)
            {
                if (_iconInstance.TryGetComponent(out SpriteRenderer iconRenderer))
                {
                    iconRenderer.color = Color.gray;
                }

                return;
            }

            if (_fallbackRenderer != null)
            {
                _fallbackRenderer.color = Color.gray;
            }
        }
    }
}
