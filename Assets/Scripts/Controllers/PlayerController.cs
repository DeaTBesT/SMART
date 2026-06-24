using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Managers;
using UnityEngine;

namespace Controllers
{
    public class PlayerController : Controller
    {
        [Header("Layers")] [SerializeField] private LayerMask _groundMask;
        [SerializeField] private LayerMask _areaLayer;
        [SerializeField] private LayerMask _selectingAreaLayer;

        [SerializeField] private float _timeDelay;

        private bool _isFirstPressed;
        private bool _isAreaMoving;

        private Vector2 _offset;
        private Camera _camera;
        private int _mapSizeX;
        private int _mapSizeY;
        private CancellationTokenSource _pressDelayCts;

        private void Start()
        {
            _camera = Camera.main;
            _mapSizeX = MapBuilder.Instance.MapSizeX;
            _mapSizeY = MapBuilder.Instance.MapSizeY;
            _pressDelayCts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _pressDelayCts?.Cancel();
            _pressDelayCts?.Dispose();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && CurrentArea == null)
            {
                TrySelectOwnedArea();
            }

            if (Input.GetMouseButtonDown(0) && CurrentArea != null)
            {
                HandlePrimaryClick();
            }

            if (Input.GetMouseButtonDown(0))
            {
                StartDrag();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isAreaMoving = false;
            }

            if (Input.GetMouseButton(0))
            {
                ContinueDrag();
            }

            // if (Input.GetMouseButtonDown(1))
            // {
            //     RotateCurrentArea();
            // }
        }

        private void HandlePrimaryClick()
        {
            var hit = CastRay(_areaLayer);
            if (!hit)
            {
                return;
            }

            var area = hit.transform.GetComponentInParent<Area>();
            if (area == null || area.IsPlaced)
            {
                return;
            }

            if (!_isFirstPressed)
            {
                StartPressDelay();
                return;
            }

            var currentArea = CurrentArea;
            if (currentArea.PlacingArea())
            {
                _isAreaMoving = false;
                _vacantCells.AddRange(currentArea.Cells);
                CancelPressDelay();
                _isFirstPressed = false;
                UpdateVacantCells();
            }
        }

        private void StartDrag()
        {
            var hit = CastRay(_areaLayer);
            if (!hit)
            {
                return;
            }

            hit.transform.TryGetComponent(out AreaCollider aCollider);
            var area = aCollider?.Area;

            if (area == null || area.IsPlaced)
            {
                return;
            }

            CurrentArea = area;
            // compute offset relative to nearest map cell to snap correctly
            if (MapBuilder.Instance.TryGetNearestCell(hit.point, out var rc))
            {
                _offset = (Vector2)CurrentArea.transform.position - (Vector2)rc.transform.position;
            }
            else
            {
                _offset = (Vector2)CurrentArea.transform.position - hit.point;
            }

            _isAreaMoving = true;
        }

        private void ContinueDrag()
        {
            if (!_isAreaMoving || CurrentArea == null)
            {
                return;
            }

            var hit = CastRay(_groundMask);
            if (!hit)
            {
                return;
            }

            var target = hit.point;
            if (MapBuilder.Instance.TryGetNearestCell(target, out var rc))
            {
                CurrentArea.transform.position = (Vector2)rc.transform.position + _offset;
            }
            else
            {
                var point = target + _offset;
                CurrentArea.transform.position = new Vector2(
                    Mathf.Clamp(point.x, -_mapSizeX, _mapSizeX),
                    Mathf.Clamp(point.y, -_mapSizeY, _mapSizeY));
            }

            CurrentArea.SetPivotPosition();
        }

        public void RotateCurrentArea()
        {
            if (CurrentArea != null && !CurrentArea.IsPlaced)
            {
                CurrentArea.Rotate();
                return;
            }

            var hit = CastRay(_areaLayer);
            if (!hit.transform)
            {
                return;
            }

            hit.transform.TryGetComponent(out AreaCollider aCollider);
            var area = aCollider?.Area;
            if (area != null && !area.IsPlaced)
            {
                area.Rotate();
            }
        }

        private RaycastHit2D CastRay(LayerMask layerMask)
        {
            return Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity,
                layerMask);
        }

        private void StartPressDelay()
        {
            _isFirstPressed = true;
            _pressDelayCts?.Cancel();
            _pressDelayCts?.Dispose();
            _pressDelayCts = new CancellationTokenSource();
            PressDelayAsync(_pressDelayCts.Token).Forget();
        }

        private async UniTaskVoid PressDelayAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_timeDelay), cancellationToken: cancellationToken);
                _isFirstPressed = false;
            }
            catch (OperationCanceledException)
            {
                _isFirstPressed = false;
            }
        }

        private void CancelPressDelay()
        {
            _pressDelayCts?.Cancel();
            _pressDelayCts?.Dispose();
            _pressDelayCts = new CancellationTokenSource();
        }

        private void TrySelectOwnedArea()
        {
            var hit = CastRay(_selectingAreaLayer);
            if (!hit)
            {
                AreaSelectionManager.Instance?.ClearSelection();
                return;
            }

            hit.transform.TryGetComponent(out AreaCollider areaCollider);
            var area = areaCollider?.Area;
            if (area == null || !area.IsPlaced || area.Controller != this)
            {
                return;
            }

            AreaSelectionManager.Instance?.ClearSelection();
            AreaSelectionManager.Instance?.SelectArea(area);
        }

        public override void SetMove(Area area)
        {
            base.SetMove(area);
        }

        private void UpdateVacantCells()
        {
            _vacantCells.RemoveAll(cell => !GameManager.Instance.CheckVacantCell(cell));
        }
    }
}