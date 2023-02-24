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

    private void Start()
    {
        _camera = Camera.main;
        _collder.size = new Vector2(sizeX, sizeY);

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

        BuildMap();
        SetupCamera();
    }

    private void BuildMap()
    {
        parentMap = new GameObject("Map");

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Instantiate(prefabCell, new Vector2(x, y) * spacing, Quaternion.identity, parentMap.transform);
            }
        }

        parentMap.transform.position = new Vector2(parentMap.transform.position.x - sizeX / 2f, parentMap.transform.position.y - sizeY / 2f) * spacing; 
    }

    private void SetupCamera()
    {
        float _x = sizeX / 2f;
        float _y = sizeY / 2f;

        //_camera.transform.position = new Vector3(_x, _y, -10);
        _camera.orthographicSize = sizeX * sizeY * sizeOffset;
    }
}
