using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform _areaPivot;
    [SerializeField] private BoxCollider2D _collider2d;
    [SerializeField] private LayerMask _areaMask;
    [SerializeField] private bool _isDebug;

    public bool IsPlaced { get; private set; }
    public AreaCollider AreaCollider { get; set; }
    public List<Transform> Cells => _cells;
    public Vector2 StartPoint => _startPoint;
    public Vector2 EndPoint => _endPoint;

    private float _currentRotationZ;
    private Controller _controller;
    private List<Transform> _cells;
    private int _sizeX;
    private int _sizeY;
    private int _mapSizeX;
    private int _mapSizeY;
    private Vector2 _startPoint;
    private Vector2 _endPoint;
    private Vector2 _raySize;
    private Vector2 _pointPosition;

    public Controller Controller
    {
        get => _controller;
        set
        {
            _controller = value;
            Redraw(_controller.TeamColor);
        }
    }

    private void OnEnable()
    {
        _cells = new List<Transform>();
    }

    public void GenerateArea(GameObject cellPrefab, int sizeX, int sizeY)
    {
        _cells = new List<Transform>(sizeX * sizeY);

        for (var x = 0; x < sizeX; x++)
        {
            for (var y = 0; y < sizeY; y++)
            {
                _cells.Add(Instantiate(cellPrefab, new Vector2(x, y), Quaternion.identity, _areaPivot).transform);
            }
        }

        _areaPivot.position = Vector2.zero;
        _collider2d.offset = new Vector2((sizeX - 1) / 2f, (sizeY - 1) / 2f);
        _collider2d.size = new Vector2(sizeX - 0.1f, sizeY - 0.1f);

        _sizeX = sizeX;
        _sizeY = sizeY;
        _mapSizeX = MapBuilder.Instance.MapSizeX / 2;
        _mapSizeY = MapBuilder.Instance.MapSizeY / 2;

        _startPoint = Vector2.zero;
        _endPoint = new Vector2(sizeX - 1, sizeY - 1);
        _raySize = new Vector2(sizeX, sizeY);
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
        _pointPosition = new Vector2(
            Mathf.Clamp(transform.position.x, -_mapSizeX - _startPoint.x, _mapSizeX - (_endPoint.x + 1)),
            Mathf.Clamp(transform.position.y, -_mapSizeY - _startPoint.y, _mapSizeY - (_endPoint.y + 1)));

        UpdateAreaPosition();
    }

    private void UpdateAreaPosition()
    {
        _areaPivot.position = new Vector2(
            Mathf.RoundToInt(_pointPosition.x),
            Mathf.RoundToInt(_pointPosition.y));
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
        Controller.AddScore(1);
        GameManager.Instance.EndMove(this);

        return true;
    }

    private bool CheckIntersections()
    {
        var u = CastBox(new Vector2(_areaPivot.position.x + _startPoint.x + _raySize.x / 2 - 0.5f, _areaPivot.position.y + _startPoint.y + _raySize.y),
            new Vector2(_raySize.x - 0.1f, 0.9f));

        var d = CastBox(new Vector2(_areaPivot.position.x + _startPoint.x + _raySize.x / 2 - 0.5f, _areaPivot.position.y + _startPoint.y - 1),
            new Vector2(_raySize.x - 0.1f, 0.9f));

        var r = CastBox(new Vector2(_areaPivot.position.x + _startPoint.x + _raySize.x, _areaPivot.position.y + _startPoint.y + _raySize.y / 2 - 0.5f),
            new Vector2(0.9f, _raySize.y - 0.1f));

        var l = CastBox(new Vector2(_areaPivot.position.x + _startPoint.x - 1, _areaPivot.position.y + _startPoint.y + _raySize.y / 2 - 0.5f),
            new Vector2(0.9f, _raySize.y - 0.1f));

        var c = CastBox(new Vector2(_areaPivot.position.x + _startPoint.x + _raySize.x / 2 - 0.5f, _areaPivot.position.y + _startPoint.y + _raySize.y / 2 - 0.5f),
            new Vector2(_raySize.x - 0.1f, _raySize.y - 0.1f));

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

        Gizmos.DrawCube(new Vector2(_areaPivot.position.x + _startPoint.x + _raySize.x / 2 - 0.5f, _areaPivot.position.y + _startPoint.y + _raySize.y), new Vector2(_raySize.x, 0.9f));
        Gizmos.DrawCube(new Vector2(_areaPivot.position.x + _startPoint.x + _raySize.x / 2 - 0.5f, _areaPivot.position.y + _startPoint.y - 1), new Vector2(_raySize.x, 0.9f));
        Gizmos.DrawCube(new Vector2(_areaPivot.position.x + _startPoint.x + _raySize.x, _areaPivot.position.y + _startPoint.y + _raySize.y / 2 - 0.5f), new Vector2(0.9f, _raySize.y));
        Gizmos.DrawCube(new Vector2(_areaPivot.position.x + _startPoint.x - 1, _areaPivot.position.y + _startPoint.y + _raySize.y / 2 - 0.5f), new Vector2(0.9f, _raySize.y));
        Gizmos.DrawCube(new Vector2(_areaPivot.position.x + _startPoint.x + _raySize.x / 2 - 0.5f, _areaPivot.position.y + _startPoint.y + _raySize.y / 2 - 0.5f), new Vector2(_raySize.x - 0.1f, _raySize.y - 0.1f));
    }
}
