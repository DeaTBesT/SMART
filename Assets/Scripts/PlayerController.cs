using System.Collections;
using UnityEngine;

public class PlayerController : Controller
{
    [Header("Layers")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask areaLayer;

    [SerializeField] private float timeDelay;

    private bool isFirstPressed = false;
    private bool isAreaMoving = false;

    private Vector2 offset;

    private Camera _camera;

    private int mapSizeX;
    private int mapSizeY;

    private void Start()
    {
        _camera = Camera.main;

        mapSizeX = MapBuilder.Instance.MapSizeX;
        mapSizeY = MapBuilder.Instance.MapSizeY;
    }

    private void Update()
    {
        if ((Input.GetMouseButtonDown(0)) && (CurrentArea != null))
        {
            RaycastHit2D hit = CastRay(areaLayer);

            if (hit)
            {
                Area area = hit.transform.GetComponentInParent<Area>();

                if ((area != null) && (!area.IsPlaced))
                {
                    if (!isFirstPressed)
                    {
                        StartCoroutine(PressDelay());
                    }
                    else
                    {
                        CurrentArea.PlacingArea();
                        StopAllCoroutines();
                        isFirstPressed = false;
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = CastRay(areaLayer);

            if (hit)
            {
                Vector2 point = hit.point;
                hit.transform.TryGetComponent(out AreaCollider aCollider);
                Area area = aCollider.GetArea;

                if ((area != null) && (!area.IsPlaced))
                {
                    CurrentArea = area;
                    offset = (Vector2)CurrentArea.transform.position - point;
                    isAreaMoving = true;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isAreaMoving = false;
        }

        if (Input.GetMouseButton(0))
        {
            RaycastHit2D hit = CastRay(groundMask);

            if ((hit) && (CurrentArea != null) && (isAreaMoving))
            {
                Vector2 point = hit.point + offset;
                              
                CurrentArea.SetPivotPosition();
                CurrentArea.transform.position = new Vector2(Mathf.Clamp(point.x, -mapSizeX, mapSizeX),
                    Mathf.Clamp(point.y, -mapSizeY, mapSizeY));
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = CastRay(areaLayer);

            if (hit.transform != null)
            {
                hit.transform.TryGetComponent(out AreaCollider aCollider);
                Area area = aCollider.GetArea;

                if ((area != null) && (!area.IsPlaced))
                {
                    area.Rotate();
                }
            }
        }
    }

    private RaycastHit2D CastRay(LayerMask layerMask)
    {
        RaycastHit2D hit;

        hit = Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, layerMask);

        return hit;
    }

    private IEnumerator PressDelay()
    {
        isFirstPressed = true;

        yield return new WaitForSeconds(timeDelay);

        isFirstPressed = false;
    }
}
