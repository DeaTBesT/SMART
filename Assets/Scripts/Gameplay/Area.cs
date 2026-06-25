using System.Collections.Generic;
using Controllers;
using Managers;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class Area : MonoBehaviour
    {
        [SerializeField] private Transform _areaPivot;
        [SerializeField] private BoxCollider2D _collider2d;
        [SerializeField] private LayerMask _areaMask;
        [SerializeField] private bool _isDebug;
        [SerializeField] private RectTransform _upgradeLabelRoot;
        [SerializeField] private TextMeshProUGUI _upgradeLevelText;

        [SerializeField] private bool _isPlaced;
        [SerializeField] public bool _isCanUpgraded = true;

        [SerializeField] private ParticleSystem _particleUpgrade;
        
        public bool IsPlaced { get => _isPlaced; private set => _isPlaced = value; }
        public AreaCollider AreaCollider { get; set; }
        public List<Transform> Cells => _cells ??= new List<Transform>();
        public Vector2 StartPoint => _startPoint;
        public Vector2 EndPoint => _endPoint;
        public int UpgradeLevel => _upgradeLevel + 1;
        public bool CanUpgrade => IsPlaced && _isCanUpgraded;

        private float _currentRotationZ;
        [SerializeField] private Controller _controller;
        [SerializeField] private List<Transform> _cells;
        private int _sizeX;
        private int _sizeY;
        private int _mapSizeX;
        private int _mapSizeY;
        private Vector2 _startPoint;
        private Vector2 _endPoint;
        private Vector2 _raySize;
        private Vector2 _pointPosition;
        private int _upgradeLevel;
        private bool _isSelected;
        private Color _originalColor;
        
        public Controller Controller
        {
            get => _controller;
            set
            {
                _controller = value;
                if (IsPlaced)
                {
                    Redraw(_controller.TeamColor);
                }
                else
                {
                    Redraw(Color.yellow);
                }
            }
        }

        public void Upgrade()
        {
            _upgradeLevel++;
            UpdateUpgradeLabel();

            if (_controller != null)
            {
                var upgradeScore = Cells.Count * 2;
                _controller.AddScore(upgradeScore);
            }
        }

        private void OnEnable()
        {
            _cells = new List<Transform>();
            UpdateUpgradeLabel();
            _originalColor = _controller?.TeamColor ?? Color.white;
        }
        
        private void OnDisable()
        {
            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            if (isSelected)
            {
                var highlightColor = _controller.TeamColor * 1.5f;
                highlightColor.a = 1f;
                Redraw(highlightColor);
            }
            else
            {
                Redraw(_controller.TeamColor);
            }
        }
        
        public void GenerateArea(GameObject cellPrefab, int sizeX, int sizeY)
        {
            _cells = new List<Transform>(sizeX * sizeY);

            var spacing = MapBuilder.Instance?.Spacing ?? 1f;
            _areaPivot.localPosition = Vector3.zero;

            for (var x = 0; x < sizeX; x++)
            {
                for (var y = 0; y < sizeY; y++)
                {
                    var go = Instantiate(cellPrefab, _areaPivot);
                    go.transform.localPosition = new Vector3(x * spacing, y * spacing, 0f);
                    _cells.Add(go.transform);
                }
            }

            _collider2d.offset = new Vector2((sizeX - 1) / 2f * spacing, (sizeY - 1) / 2f * spacing);
            _collider2d.size = new Vector2(sizeX * spacing - 0.1f, sizeY * spacing - 0.1f);

            _sizeX = sizeX;
            _sizeY = sizeY;
            _mapSizeX = MapBuilder.Instance.MapSizeX / 2;
            _mapSizeY = MapBuilder.Instance.MapSizeY / 2;

            _startPoint = Vector2.zero;
            _endPoint = new Vector2(sizeX - 1, sizeY - 1);
            _raySize = new Vector2(sizeX, sizeY);

            var shape =_particleUpgrade.shape; 
            shape.scale = _collider2d.size;
            
            UpdateUpgradeLabel();
        }

        public void Rotate()
        {
            _currentRotationZ += 90;
            _areaPivot.rotation = Quaternion.AngleAxis(_currentRotationZ, Vector3.forward);

            switch (_currentRotationZ % 360)
            {
                default:
                    _startPoint = Vector2.zero;
                    _endPoint = new Vector2(_sizeX - 1, _sizeY - 1);
                    _raySize = new Vector2(_sizeX, _sizeY);
                    break;
                case 90:
                    _startPoint = new Vector2(-_sizeY + 1, 0);
                    _endPoint = new Vector2(0, _sizeX - 1);
                    _raySize = new Vector2(_sizeY, _sizeX);
                    break;
                case 180:
                    _startPoint = new Vector2(-_sizeX + 1, -_sizeY + 1);
                    _endPoint = Vector2.zero;
                    _raySize = new Vector2(_sizeX, _sizeY);
                    break;
                case 270:
                    _startPoint = new Vector2(0, -_sizeX + 1);
                    _endPoint = new Vector2(_sizeY - 1, 0);
                    _raySize = new Vector2(_sizeY, _sizeX);
                    break;
            }

            SetPivotPosition();
            UpdateAreaPosition();
            UpdateUpgradeLabel();
        }

        public void AddCell(Transform cell)
        {
            _cells.Add(cell);
        }

        private void Redraw(Color color)
        {
            foreach (var cell in _cells)
            {
                if (cell.TryGetComponent(out SpriteRenderer spriteRenderer))
                {
                    spriteRenderer.color = color;
                }
            }
        }

        public void SetPivotPosition()
        {
            var worldPos = transform.position;
            if (MapBuilder.Instance.TryGetNearestCell(worldPos, out var rc))
            {
                transform.position = rc.transform.position;
                _pointPosition = rc.transform.position;
            }

            UpdateAreaPosition();
        }

        private void UpdateAreaPosition()
        {
            // area pivot uses local coordinates; align pivot world position with the area transform
            _areaPivot.position = transform.position;
        }

        private void LateUpdate()
        {
            UpdateUpgradeLabel();
        }

        private void UpdateUpgradeLabel()
        {
            if (_upgradeLevelText == null || _upgradeLabelRoot == null)
            {
                return;
            }

            _upgradeLevelText.text = UpgradeLevel.ToString();

            // Calculate center of all cells
            if (_cells.Count == 0)
            {
                return;
            }

            Vector2 centerSum = Vector2.zero;
            foreach (var cell in _cells)
            {
                centerSum += (Vector2)cell.position;
            }
            var areaCenter = centerSum / _cells.Count;

            // Use anchoredPosition for UI elements
            if (_upgradeLabelRoot is RectTransform rectTransform)
            {
                rectTransform.position = areaCenter;
            }
            else
            {
                _upgradeLabelRoot.position = areaCenter;
            }
            
            _upgradeLabelRoot.rotation = Quaternion.identity;
        }

        public bool IsCanPlace()
        {
            return CheckIntersections();
        }

        public bool PlacingArea()
        {
            if (!CheckIntersections())
            {
                return false;
            }

            IsPlaced = true;
            AreaCollider.SetActiveArea(true);
            Redraw(_controller.TeamColor);
            Controller.AddScore(Cells.Count * UpgradeLevel);
            ResourceManager.Instance?.CaptureAreaResources(this);
            GameManager.Instance.EndMove(this);
            UpdateUpgradeLabel();
            
            return true;
        }

        private bool CheckIntersections()
        {
            var spacing = MapBuilder.Instance?.Spacing ?? 1f;

            var u = CastBox(
                new Vector2(_areaPivot.position.x + (_startPoint.x + _raySize.x / 2 - 0.5f) * spacing,
                    _areaPivot.position.y + (_startPoint.y + _raySize.y) * spacing),
                new Vector2((_raySize.x - 0.1f) * spacing, 0.9f * spacing));

            var d = CastBox(
                new Vector2(_areaPivot.position.x + (_startPoint.x + _raySize.x / 2 - 0.5f) * spacing,
                    _areaPivot.position.y + (_startPoint.y - 1) * spacing),
                new Vector2((_raySize.x - 0.1f) * spacing, 0.9f * spacing));

            var r = CastBox(
                new Vector2(_areaPivot.position.x + (_startPoint.x + _raySize.x) * spacing,
                    _areaPivot.position.y + (_startPoint.y + _raySize.y / 2 - 0.5f) * spacing),
                new Vector2(0.9f * spacing, (_raySize.y - 0.1f) * spacing));

            var l = CastBox(
                new Vector2(_areaPivot.position.x + (_startPoint.x - 1) * spacing,
                    _areaPivot.position.y + (_startPoint.y + _raySize.y / 2 - 0.5f) * spacing),
                new Vector2(0.9f * spacing, (_raySize.y - 0.1f) * spacing));

            var c = CastBox(
                new Vector2(_areaPivot.position.x + (_startPoint.x + _raySize.x / 2 - 0.5f) * spacing,
                    _areaPivot.position.y + (_startPoint.y + _raySize.y / 2 - 0.5f) * spacing),
                new Vector2((_raySize.x - 0.1f) * spacing, (_raySize.y - 0.1f) * spacing));

            AreaCollider uAreaCollider = null;
            AreaCollider dAreaCollider = null;
            AreaCollider rAreaCollider = null;
            AreaCollider lAreaCollider = null;
            AreaCollider cAreaCollider = null;

            u.transform?.TryGetComponent(out uAreaCollider);
            d.transform?.TryGetComponent(out dAreaCollider);
            r.transform?.TryGetComponent(out rAreaCollider);
            l.transform?.TryGetComponent(out lAreaCollider);
            c.transform?.TryGetComponent(out cAreaCollider);

            var isAdjacentToOwnCell =
                (uAreaCollider != null && uAreaCollider.Area.Controller.TeamID == Controller.TeamID) ||
                (dAreaCollider != null && dAreaCollider.Area.Controller.TeamID == Controller.TeamID) ||
                (rAreaCollider != null && rAreaCollider.Area.Controller.TeamID == Controller.TeamID) ||
                (lAreaCollider != null && lAreaCollider.Area.Controller.TeamID == Controller.TeamID);

            return isAdjacentToOwnCell && cAreaCollider == null;
        }

        private RaycastHit2D CastBox(Vector2 origin, Vector2 size)
        {
            return Physics2D.BoxCast(origin, size, 0f, _areaPivot.forward, Mathf.Infinity, _areaMask);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_isDebug)
            {
                return;
            }

            var spacing = MapBuilder.Instance?.Spacing ?? 1f;

            Gizmos.DrawCube(
                new Vector2(_areaPivot.position.x + (_startPoint.x + _raySize.x / 2 - 0.5f) * spacing,
                    _areaPivot.position.y + (_startPoint.y + _raySize.y) * spacing), new Vector2(_raySize.x * spacing, 0.9f * spacing));
            Gizmos.DrawCube(
                new Vector2(_areaPivot.position.x + (_startPoint.x + _raySize.x / 2 - 0.5f) * spacing,
                    _areaPivot.position.y + (_startPoint.y - 1) * spacing), new Vector2(_raySize.x * spacing, 0.9f * spacing));
            Gizmos.DrawCube(
                new Vector2(_areaPivot.position.x + (_startPoint.x + _raySize.x) * spacing,
                    _areaPivot.position.y + (_startPoint.y + _raySize.y / 2 - 0.5f) * spacing), new Vector2(0.9f * spacing, _raySize.y * spacing));
            Gizmos.DrawCube(
                new Vector2(_areaPivot.position.x + (_startPoint.x - 1) * spacing,
                    _areaPivot.position.y + (_startPoint.y + _raySize.y / 2 - 0.5f) * spacing), new Vector2(0.9f * spacing, _raySize.y * spacing));
            Gizmos.DrawCube(
                new Vector2(_areaPivot.position.x + (_startPoint.x + _raySize.x / 2 - 0.5f) * spacing,
                    _areaPivot.position.y + (_startPoint.y + _raySize.y / 2 - 0.5f) * spacing),
                new Vector2((_raySize.x - 0.1f) * spacing, (_raySize.y - 0.1f) * spacing));
        }
    }
}