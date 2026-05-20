using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerController : Controller
{
    [Header("Layers")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private LayerMask _areaLayer;

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

        if (Input.GetMouseButtonDown(1))
        {
            RotateCurrentArea();
        }
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

        if (CurrentArea.PlacingArea())
        {
            _vacantCells.AddRange(CurrentArea.Cells);
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
        _offset = (Vector2)CurrentArea.transform.position - hit.point;
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

        var point = hit.point + _offset;
        CurrentArea.SetPivotPosition();
        CurrentArea.transform.position = new Vector2(
            Mathf.Clamp(point.x, -_mapSizeX, _mapSizeX),
            Mathf.Clamp(point.y, -_mapSizeY, _mapSizeY));
    }

    private void RotateCurrentArea()
    {
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
        return Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, layerMask);
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

    public override void SetMove(Area area)
    {
        base.SetMove(area);
    }

    private void UpdateVacantCells()
    {
        _vacantCells.RemoveAll(cell => !GameManager.Instance.CheckVacantCell(cell));
    }
}

