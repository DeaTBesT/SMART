using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : Singleton<MapBuilder>
{
    [Header("Map settings")]
    [SerializeField] private float spacing;
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;
    [SerializeField] private GameObject prefabCell;
    [SerializeField] private BoxCollider2D _collder;

    [Header("Camera settings")]
    [SerializeField] private float sizeOffset = 1;

    private GameObject parentMap;
    private Camera _camera;

    public int MapSizeX => sizeX;
    public int MapSizeY => sizeY;

    public Corner[] Corners => corners;
    private Corner[] corners;

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
        _collder.size = new Vector2(sizeX, sizeY);

        SetupCorners();
        BuildMap();
        SetupCamera();
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
        Destroy(parentMap);

        SetupCorners();
        BuildMap();      
        SetupCamera();
    }

    private void BuildMap()
    {
        parentMap = new GameObject("Map");

        BuildCornens();

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Instantiate(prefabCell, new Vector2(x, y) * spacing, Quaternion.identity, parentMap.transform);
            }
        }

        parentMap.transform.position = new Vector2(Mathf.RoundToInt(parentMap.transform.position.x - sizeX / 2f), Mathf.RoundToInt(parentMap.transform.position.y - sizeY / 2f)) * spacing;

        AddVacantPlaces();
    }

    private void SetupCorners()
    {
        corners = new Corner[4];
        
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i].cellPositions = new Vector2[3];
            corners[i].cells = new GameObject[3];
            corners[i].vacantCells = new List<Transform>();
        }

        //LD
        corners[0].cellPositions[0] = new Vector2(-1, -1);
        corners[0].cellPositions[1] = new Vector2(0, -1);
        corners[0].cellPositions[2] = new Vector2(-1, 0);

        //RD
        corners[1].cellPositions[0] = new Vector2(sizeX, -1);
        corners[1].cellPositions[1] = new Vector2(sizeX - 1, -1);
        corners[1].cellPositions[2] = new Vector2(sizeX, 0);

        //LU
        corners[2].cellPositions[0] = new Vector2(-1, sizeY);
        corners[2].cellPositions[1] = new Vector2(-1, sizeY - 1);
        corners[2].cellPositions[2] = new Vector2(0, sizeY);

        //RU
        corners[3].cellPositions[0] = new Vector2(sizeX, sizeY);
        corners[3].cellPositions[1] = new Vector2(sizeX - 1, sizeY);
        corners[3].cellPositions[2] = new Vector2(sizeX, sizeY - 1);
    }

    private void AddVacantPlaces()
    {
        for (int i = 0; i < corners.Length; i++)
        {
            Corner corner = corners[i];

            for (int j = 0; j < corner.cells.Length; j++)
            {
                corner.vacantCells.Add(corner.cells[j].transform);
            }
        }
    }

    private void BuildCornens()
    {
        for (int i = 0; i < corners.Length; i++)
        {
            GameObject areaCorner = new GameObject($"Corner {i}");
            areaCorner.transform.parent = parentMap.transform;
            corners[i].areaCorner = areaCorner.AddComponent<Area>();

            Corner corner = corners[i];

            for (int j = 0; j < corner.cellPositions.Length; j++)   
            {
                GameObject cell = Instantiate(prefabCell, new Vector2(corner.cellPositions[j].x, corner.cellPositions[j].y), Quaternion.identity, areaCorner.transform);

                corner.cells[j] = cell;

                cell.AddComponent<AreaCollider>();
                cell.TryGetComponent(out AreaCollider aCollider);

                aCollider.SetActiveArea(false);
                aCollider.SetArea(corner.areaCorner);
                corner.areaCorner.AddCell(cell.transform);

                cell.TryGetComponent(out SpriteRenderer sRenderer);
                sRenderer.color = Color.black;
            }   
        }
    }

    private void SetupCamera()
    {
        float _x = sizeX / 2f;
        float _y = sizeY / 2f;

        //_camera.transform.position = new Vector3(_x, _y, -10);
        _camera.orthographicSize = sizeX * sizeY * sizeOffset;
    }
}
