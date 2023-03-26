using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform areaPivot;
    [SerializeField] private BoxCollider2D _collider2d;
    [SerializeField] private LayerMask areaMask;

    [SerializeField] private bool isDebug;

    private bool isPlaced = false;
    public bool IsPlaced => isPlaced;

    private Vector2 pivotOffset;

    private float currentRotationZ;

    private Controller controller;
    public Controller Controller 
    {
        get => controller;
        set
        {
            controller = value;
            Redraw(controller.TeamColor);
        } 
    }

    public AreaCollider m_AreaCollider { get; set; }

    private List<Transform> cells;
    public List<Transform> GetCells => cells;

    private int sizeX;
    private int sizeY;

    private int mapSizeX;
    private int mapSizeY;

    private Vector2 startPoint;
    private Vector2 endPoint;
    private Vector2 raySize;

    private Vector2 pointPosition;

    private void OnEnable()
    {
        cells = new List<Transform>();
    }

    public void GenerateArea(GameObject cellPrefab, int sizeX, int sizeY)
    {

#if UNITY_EDITOR

        Debug.Log($"Area setup; SizeX : {sizeX}; SizeY : {sizeY}; Square : {sizeX * sizeY}");

#endif

        for (int x = 0; x < sizeX; x++)
        {
             for (int y = 0; y < sizeY; y++)
            {            
                cells.Add(Instantiate(cellPrefab, new Vector2(x, y), Quaternion.identity, areaPivot).transform);
            }
        }

        //pivotOffset = new Vector2(areaPivot.transform.position.x - sizeX / 2, areaPivot.transform.position.y - sizeY/ 2);
        areaPivot.transform.position = pivotOffset;
        _collider2d.offset = new Vector2((sizeX - 1) / 2f, (sizeY - 1) / 2f);
        _collider2d.size = new Vector2(sizeX - 0.1f, sizeY - 0.1f);

        this.sizeX = sizeX;
        this.sizeY = sizeY;

        mapSizeX = MapBuilder.Instance.MapSizeX / 2;
        mapSizeY = MapBuilder.Instance.MapSizeY / 2;

        startPoint.x = 0;
        startPoint.y = 0;

        endPoint.x = sizeX - 1;
        endPoint.y = sizeY - 1;

        raySize.x = sizeX;
        raySize.y = sizeY;
    }

    public void Rotate()//0, 90, 180, 270
    {
        currentRotationZ += 90;

        areaPivot.rotation = Quaternion.AngleAxis(currentRotationZ, Vector3.forward);

        startPoint = Vector2.zero;
        endPoint = Vector2.zero;

        switch (currentRotationZ % 360)
        {
            case 0:
                {
                    startPoint.x = 0;
                    startPoint.y = 0;

                    endPoint.x = sizeX - 1;
                    endPoint.y = sizeY - 1;

                    raySize.x = sizeX;
                    raySize.y = sizeY;
                }
                break;
            case 90:
                {
                    startPoint.x = -sizeY + 1;
                    startPoint.y = 0;

                    endPoint.x = 0;
                    endPoint.y = sizeX - 1;

                    raySize.x = sizeY;
                    raySize.y = sizeX;
                }
                break;
            case 180:
                {
                    startPoint.x = -sizeX + 1;
                    startPoint.y = -sizeY + 1;

                    endPoint.x = 0;
                    endPoint.y = 0;

                    raySize.x = sizeX;
                    raySize.y = sizeY;
                }
                break;
            case 270:
                {
                    startPoint.x = 0;
                    startPoint.y = -sizeX + 1;

                    endPoint.x = sizeY - 1;
                    endPoint.y = 0;

                    raySize.x = sizeY;
                    raySize.y = sizeX;
                }
                break;
        }

        SetPivotPosition();
        UpdateAreaPosition();
    }

    public void AddCell(Transform cell)
    {
        cells.Add(cell);
    }

    private void Redraw(Color color)
    {
        foreach (Transform cell in cells)
        {
            cell.TryGetComponent(out SpriteRenderer sprite);
            sprite.color = color;
        }
    }

    public void SetPivotPosition()
    {
        pointPosition = new Vector2(Mathf.Clamp(transform.position.x, -mapSizeX - startPoint.x, mapSizeX - (endPoint.x + 1)),
            Mathf.Clamp(transform.position.y, -mapSizeY - startPoint.y, mapSizeY - (endPoint.y + 1)));

        UpdateAreaPosition();
    }

    private void UpdateAreaPosition()
    {
        areaPivot.position = new Vector2(Mathf.RoundToInt(pointPosition.x),
            Mathf.RoundToInt(pointPosition.y));
    }

    public bool PlacingArea()
    {
        if (!CheckIntersections())
        {
            return false;   
        }

        isPlaced = true;

        m_AreaCollider.SetActiveArea(true);
        Controller.AddScore(1);
        GameManager.Instance.EndMove();

        return true;
    }

    private bool CheckIntersections()
    {
        RaycastHit2D u = Physics2D.BoxCast(new Vector2(areaPivot.position.x + startPoint.x + raySize.x / 2 - 0.5f, areaPivot.position.y + startPoint.y + raySize.y),
            new Vector2(raySize.x - 0.1f, 0.9f), 0, areaPivot.forward, Mathf.Infinity, areaMask);

        RaycastHit2D d = Physics2D.BoxCast(new Vector2(areaPivot.position.x + startPoint.x + raySize.x / 2 - 0.5f, areaPivot.position.y + startPoint.y - 1),
            new Vector2(raySize.x - 0.1f, 0.9f), 0, areaPivot.forward, Mathf.Infinity, areaMask);

        RaycastHit2D r = Physics2D.BoxCast(new Vector2(areaPivot.position.x + startPoint.x + raySize.x, areaPivot.position.y + startPoint.y + raySize.y / 2 - 0.5f),
            new Vector2(0.9f, raySize.y - 0.1f), 0, areaPivot.forward, Mathf.Infinity, areaMask);

        RaycastHit2D l = Physics2D.BoxCast(new Vector2(areaPivot.position.x + startPoint.x - 1, areaPivot.position.y + startPoint.y + raySize.y / 2 - 0.5f),
            new Vector2(0.9f, raySize.y - 0.1f), 0, areaPivot.forward, Mathf.Infinity, areaMask);

        RaycastHit2D c = Physics2D.BoxCast(new Vector2(areaPivot.position.x + startPoint.x + raySize.x / 2 - 0.5f, areaPivot.position.y + startPoint.y + raySize.y / 2 - 0.5f),
            new Vector2(raySize.x - 0.1f, raySize.y - 0.1f), 0, areaPivot.forward, Mathf.Infinity, areaMask);//Center

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

        if ((((uAreaCollider != null) && (uAreaCollider.GetArea.Controller.TeamID == Controller.TeamID)) ||
            ((dAreaCollider != null) && (dAreaCollider.GetArea.Controller.TeamID == Controller.TeamID)) ||
            ((rAreaCollider != null) && (rAreaCollider.GetArea.Controller.TeamID == Controller.TeamID)) ||
            ((lAreaCollider != null) && (lAreaCollider.GetArea.Controller.TeamID == Controller.TeamID))) &&
            (cAreaCollider == null))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!isDebug) { return; }

        Gizmos.DrawCube(new Vector2(areaPivot.position.x + startPoint.x + raySize.x / 2 - 0.5f, areaPivot.position.y + startPoint.y + raySize.y), new Vector2(raySize.x, 0.9f)) ; //Up
        Gizmos.DrawCube(new Vector2(areaPivot.position.x + startPoint.x + raySize.x / 2 - 0.5f, areaPivot.position.y + startPoint.y - 1), new Vector2(raySize.x, 0.9f)); //Down
        Gizmos.DrawCube(new Vector2(areaPivot.position.x + startPoint.x + raySize.x, areaPivot.position.y + startPoint.y + raySize.y / 2 - 0.5f), new Vector2(0.9f, raySize.y)); //Right
        Gizmos.DrawCube(new Vector2(areaPivot.position.x + startPoint.x - 1, areaPivot.position.y + startPoint.y + raySize.y / 2 - 0.5f), new Vector2(0.9f, raySize.y)); //Left
        Gizmos.DrawCube(new Vector2(areaPivot.position.x + startPoint.x + raySize.x / 2 - 0.5f, areaPivot.position.y + startPoint.y + raySize.y / 2 - 0.5f), new Vector2(raySize.x - 0.1f, raySize.y - 0.1f));//Center
    }
}
