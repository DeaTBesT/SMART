using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform areaPivot;
    [SerializeField] private BoxCollider2D _collider2d;
    [SerializeField] private LayerMask areaMask;

    private bool isPlaced = false;
    public bool IsPlaced => isPlaced;

    private Vector2 pivotOffset;

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

    private List<GameObject> cells;

    private void OnEnable()
    {
        cells = new List<GameObject>();
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
                cells.Add(Instantiate(cellPrefab, new Vector2(x, y), Quaternion.identity, areaPivot));
            }
        }

        //pivotOffset = new Vector2(areaPivot.transform.position.x - sizeX / 2, areaPivot.transform.position.y - sizeY/ 2);
        areaPivot.transform.position = pivotOffset;
        _collider2d.offset = new Vector2((sizeX - 1) / 2f, (sizeY - 1) / 2f);
        _collider2d.size = new Vector2(sizeX, sizeY);
    }

    public void AddCell(GameObject cell)
    {
        cells.Add(cell);
    }

    private void Redraw(Color color)
    {
        foreach (GameObject cell in cells)
        {
            cell.TryGetComponent(out SpriteRenderer sprite);
            sprite.color = color;
        }
    }

    public void SetPivotPosition()
    {
        areaPivot.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    public bool PlacingArea()
    {
        if (!CheckIntersections())
        {
            return false;   
        }

        isPlaced = true;

        Controller.AddScore(1);
        GameManager.Instance.EndMove();

        return true;
    }

    private bool CheckIntersections()
    {
        RaycastHit2D u = Physics2D.BoxCast(new Vector2(areaPivot.position.x + _collider2d.size.x / 2 - 0.5f, areaPivot.position.y + _collider2d.size.y), 
            new Vector2(_collider2d.size.x - 0.1f, 0.9f), 0, areaPivot.forward, Mathf.Infinity, areaMask);

        RaycastHit2D d = Physics2D.BoxCast(new Vector2(areaPivot.position.x + _collider2d.size.x / 2 - 0.5f, areaPivot.position.y - 1),
            new Vector2(_collider2d.size.x - 0.1f, 0.9f), 0, areaPivot.forward, Mathf.Infinity, areaMask);

        RaycastHit2D r = Physics2D.BoxCast(new Vector2(areaPivot.position.x + _collider2d.size.x, areaPivot.position.y + _collider2d.size.y / 2 - 0.5f),
            new Vector2(0.9f, _collider2d.size.y - 0.1f), 0, areaPivot.forward, Mathf.Infinity, areaMask);

        RaycastHit2D l = Physics2D.BoxCast(new Vector2(areaPivot.position.x - 1, areaPivot.position.y + _collider2d.size.y / 2 - 0.5f),
            new Vector2(0.9f, _collider2d.size.y - 0.1f), 0, areaPivot.forward, Mathf.Infinity, areaMask);

        AreaCollider uAreaCollider = null;
        AreaCollider dAreaCollider = null;
        AreaCollider rAreaCollider = null;
        AreaCollider lAreaCollider = null;

        u.transform?.TryGetComponent(out uAreaCollider); 
        d.transform?.TryGetComponent(out dAreaCollider);
        r.transform?.TryGetComponent(out rAreaCollider);
        l.transform?.TryGetComponent(out lAreaCollider);

        if (((uAreaCollider != null) && (uAreaCollider.GetArea.Controller.TeamID == Controller.TeamID)) ||
            ((dAreaCollider != null) && (dAreaCollider.GetArea.Controller.TeamID == Controller.TeamID)) ||
            ((rAreaCollider != null) && (rAreaCollider.GetArea.Controller.TeamID == Controller.TeamID)) ||
            ((lAreaCollider != null) && (lAreaCollider.GetArea.Controller.TeamID == Controller.TeamID)))
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
        //Gizmos.DrawCube(new Vector2(areaPivot.position.x + _collider2d.size.x / 2 - 0.5f, areaPivot.position.y + _collider2d.size.y), new Vector2(_collider2d.size.x, 0.9f)); //Up
        //Gizmos.DrawCube(new Vector2(areaPivot.position.x + _collider2d.size.x / 2 - 0.5f, areaPivot.position.y - 1), new Vector2(_collider2d.size.x, 0.9f)); //Down
        //Gizmos.DrawCube(new Vector2(areaPivot.position.x + _collider2d.size.x, areaPivot.position.y + _collider2d.size.y / 2 - 0.5f), new Vector2(0.9f, _collider2d.size.y)); //Right
        //Gizmos.DrawCube(new Vector2(areaPivot.position.x - 1, areaPivot.position.y + _collider2d.size.y / 2 - 0.5f), new Vector2(0.9f, _collider2d.size.y)); //Left
    }
}
