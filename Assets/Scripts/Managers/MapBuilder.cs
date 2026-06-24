using System.Collections.Generic;
using System.Threading;
using Bootstrap;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay;
using UnityEngine;

namespace Managers
{
    public class MapBuilder : Singleton<MapBuilder>, IInitializable
    {
        [Header("Map settings")] [SerializeField]
        private float _spacing;

        [SerializeField] private int _sizeX;
        [SerializeField] private int _sizeY;
        [SerializeField] private GameObject _prefabCell;

        [SerializeField] private List<GameObject> _cells;
        
        [Header("Camera settings")] [SerializeField]
        private float _sizeOffset = 1;

        private GameObject _parentMap;
        private Camera _camera;

        [Header("Resource settings")]
        [SerializeField] private int _resourceCount = 15;
        [SerializeField] private GameObject _resourceIconPrefab;
        [SerializeField] private ResourceType[] _resourceTypes;

        private ResourceCell[,] _gridCells;

        public int MapSizeX => _sizeX;
        public int MapSizeY => _sizeY;
        public float Spacing => _spacing;
        public Vector2 MapCenter => _parentMap != null
            ? _parentMap.transform.position + new Vector3((_sizeX - 1) * _spacing / 2f, (_sizeY - 1) * _spacing / 2f)
            : Vector2.zero;

        public Corner[] Corners => _corners;
        [SerializeField] private Corner[] _corners;

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

            SetupCorners();
            BuildMap();
            //SetupCamera();

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
            //SetupCamera();
        }

        private void BuildMap()
        {
            BuildCorners();

            _gridCells = new ResourceCell[_sizeX, _sizeY];

            for (var x = 0; x < _sizeX; x++)
            {
                for (var y = 0; y < _sizeY; y++)
                {
                    var index = x + y * _sizeX;
                    if (_cells == null || index < 0 || index >= _cells.Count)
                    {
                        _gridCells[x, y] = null;
                        continue;
                    }

                    var go = _cells[index];
                    if (go == null)
                    {
                        _gridCells[x, y] = null;
                        continue;
                    }

                    var resourceCell = go.GetComponent<ResourceCell>();
                    _gridCells[x, y] = resourceCell;
                    if (resourceCell != null)
                    {
                        resourceCell.name = $"Tile {x}x{y}";
                    }
                }
            }

            GenerateResources();
            AddVacantPlaces();
        }

        private void SetupCorners()
        {
           
        }

        private void AddVacantPlaces()
        {
            foreach (var corner in _corners)
            {
                foreach (var t in corner.cells)
                {
                    corner.vacantCells.Add(t.transform);
                }
            }
        }

        private void GenerateResources()
        {
            if (_cells == null || _cells.Count == 0)
            {
                return;
            }

            var availableCells = new List<ResourceCell>();
            foreach (var go in _cells)
            {
                if (go == null) continue;
                var rc = go.GetComponent<ResourceCell>();
                if (rc != null) availableCells.Add(rc);
            }

            if (availableCells.Count == 0)
            {
                return;
            }

            var resourcesToPlace = Mathf.Clamp(_resourceCount, 0, availableCells.Count);
            for (var i = 0; i < resourcesToPlace; i++)
            {
                var index = UnityEngine.Random.Range(0, availableCells.Count);
                var resourceCell = availableCells[index];
                availableCells.RemoveAt(index);

                var resourceType = GetRandomResourceType();
                if (resourceType == ResourceType.None)
                {
                    continue;
                }

                resourceCell.SetResource(resourceType, GetResourceAmount(resourceType), _resourceIconPrefab);
            }
        }

        private ResourceType GetRandomResourceType()
        {
            if (_resourceTypes == null || _resourceTypes.Length == 0)
            {
                return ResourceType.Wood;
            }

            return _resourceTypes[UnityEngine.Random.Range(0, _resourceTypes.Length)];
        }

        private int GetResourceAmount(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Wood => 1,
                ResourceType.Ore => 2,
                _ => 0,
            };
        }

        public bool TryGetCellAtWorldPosition(Vector2 worldPosition, out ResourceCell resourceCell)
        {
            resourceCell = null;
            if (_parentMap == null)
            {
                return false;
            }

            var localPosition = worldPosition - (Vector2)_parentMap.transform.position;
            var x = Mathf.RoundToInt(localPosition.x / _spacing);
            var y = Mathf.RoundToInt(localPosition.y / _spacing);

            if (x < 0 || x >= _sizeX || y < 0 || y >= _sizeY)
            {
                return false;
            }

            resourceCell = _gridCells[x, y];
            return true;
        }

        private void BuildCorners()
        {
            for (var i = 0; i < _corners.Length; i++)
            {
                _corners[i].areaCorner.AreaCollider.SetActiveArea(false);
            }
        }

        public bool TryGetNearestCell(Vector2 worldPosition, out ResourceCell resourceCell)
        {
            resourceCell = null;
            if (_cells == null || _cells.Count == 0)
            {
                return false;
            }

            float bestSqr = float.MaxValue;
            ResourceCell best = null;
            foreach (var go in _cells)
            {
                if (go == null) continue;
                var rc = go.GetComponent<ResourceCell>();
                if (rc == null) continue;

                var d = ((Vector2)go.transform.position - worldPosition).sqrMagnitude;
                if (d < bestSqr)
                {
                    bestSqr = d;
                    best = rc;
                }
            }

            resourceCell = best;
            return best != null;
        }

        private void SetupCamera() => 
            _camera.orthographicSize = _sizeX * _sizeY * _sizeOffset;
    }
}