using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapBuilder : Singleton<MapBuilder>, IInitializable
{
    [Header("Map settings")]
    [SerializeField] private float _spacing;
    [SerializeField] private int _sizeX;
    [SerializeField] private int _sizeY;
    [SerializeField] private GameObject _prefabCell;
    [SerializeField] private BoxCollider2D _collider;

    [Header("Camera settings")]
    [SerializeField] private float _sizeOffset = 1;

    private GameObject _parentMap;
    private Camera _camera;

    public int MapSizeX => _sizeX;
    public int MapSizeY => _sizeY;

    public Corner[] Corners => _corners;
    private Corner[] _corners;

    [System.Serializable]
    public struct Corner
    {
        public Vector2[] cellPositions;
        public GameObject[] cells;
        public Area areaCorner;
        public List<Transform> vacantCells;
    }

    private void Awake()
    {
        _camera = Camera.main;
    }

    public async UniTask InitializeAsync(CancellationToken cancellationToken)
    {
        _camera = Camera.main;
        _collider.size = new Vector2(_sizeX, _sizeY);

        SetupCorners();
        BuildMap();
        SetupCamera();

        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RebuildMap();
        }
    }
#endif

    private void RebuildMap()
    {
        Destroy(_parentMap);

        SetupCorners();
        BuildMap();
        SetupCamera();
    }

    private void BuildMap()
    {
        _parentMap = new GameObject("Map");

        BuildCorners();

        for (var x = 0; x < _sizeX; x++)
        {
            for (var y = 0; y < _sizeY; y++)
            {
                Instantiate(_prefabCell, new Vector2(x, y) * _spacing, Quaternion.identity, _parentMap.transform);
            }
        }

        _parentMap.transform.position = new Vector2(
            Mathf.RoundToInt(_parentMap.transform.position.x - _sizeX / 2f),
            Mathf.RoundToInt(_parentMap.transform.position.y - _sizeY / 2f)) * _spacing;

        AddVacantPlaces();
    }

    private void SetupCorners()
    {
        _corners = new Corner[4];

        for (var i = 0; i < _corners.Length; i++)
        {
            _corners[i].cellPositions = new Vector2[3];
            _corners[i].cells = new GameObject[3];
            _corners[i].vacantCells = new List<Transform>();
        }

        _corners[0].cellPositions[0] = new Vector2(-1, -1);
        _corners[0].cellPositions[1] = new Vector2(0, -1);
        _corners[0].cellPositions[2] = new Vector2(-1, 0);

        _corners[1].cellPositions[0] = new Vector2(_sizeX, -1);
        _corners[1].cellPositions[1] = new Vector2(_sizeX - 1, -1);
        _corners[1].cellPositions[2] = new Vector2(_sizeX, 0);

        _corners[2].cellPositions[0] = new Vector2(-1, _sizeY);
        _corners[2].cellPositions[1] = new Vector2(-1, _sizeY - 1);
        _corners[2].cellPositions[2] = new Vector2(0, _sizeY);

        _corners[3].cellPositions[0] = new Vector2(_sizeX, _sizeY);
        _corners[3].cellPositions[1] = new Vector2(_sizeX - 1, _sizeY);
        _corners[3].cellPositions[2] = new Vector2(_sizeX, _sizeY - 1);
    }

    private void AddVacantPlaces()
    {
        for (var i = 0; i < _corners.Length; i++)
        {
            var corner = _corners[i];

            for (var j = 0; j < corner.cells.Length; j++)
            {
                corner.vacantCells.Add(corner.cells[j].transform);
            }
        }
    }

    private void BuildCorners()
    {
        for (var i = 0; i < _corners.Length; i++)
        {
            ref Corner corner = ref _corners[i];

            var areaCorner = new GameObject($"Corner {i}");
            areaCorner.transform.parent = _parentMap.transform;
            corner.areaCorner = areaCorner.AddComponent<Area>();

            for (var j = 0; j < corner.cellPositions.Length; j++)
            {
                var cell = Instantiate(_prefabCell, corner.cellPositions[j], Quaternion.identity, areaCorner.transform);
                corner.cells[j] = cell;

                cell.AddComponent<AreaCollider>();
                cell.TryGetComponent(out AreaCollider areaCollider);

                areaCollider.SetActiveArea(false);
                areaCollider.SetArea(corner.areaCorner);
                corner.areaCorner.AddCell(cell.transform);

                cell.TryGetComponent(out SpriteRenderer spriteRenderer);
                spriteRenderer.color = Color.black;
            }
        }
    }

    private void SetupCamera()
    {
        _camera.orthographicSize = _sizeX * _sizeY * _sizeOffset;
    }
}
