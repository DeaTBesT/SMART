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

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if ((Input.GetMouseButtonDown(0)) && (CurrentArea != null))
        {
            if (CastRay(areaLayer))
            {
                Area area = CastRay(areaLayer).transform.GetComponentInParent<Area>();

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
            if (CastRay(areaLayer))
            {
                Vector2 point = CastRay(areaLayer).point;
                CastRay(areaLayer).transform.TryGetComponent(out AreaCollider aCollider);
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
            if ((CastRay(groundMask)) && (CurrentArea != null) && (isAreaMoving))
            {
                Vector2 point = CastRay(groundMask).point;
                              
                CurrentArea.SetPivotPosition();
                CurrentArea.transform.position = point + offset;
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
