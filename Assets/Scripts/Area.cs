using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform areaPivot;
    [SerializeField] private BoxCollider2D _collider2d;

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

    public void GenerateArea(GameObject cellPrefab, int sizeX, int sizeY)
    {

#if UNITY_EDITOR

        Debug.Log($"Area setup; SizeX : {sizeX}; SizeY : {sizeY}; Square : {sizeX * sizeY}");

#endif

        cells = new List<GameObject>();

        for (int x = 0; x < sizeX; x++)
        {
             for (int y = 0; y < sizeY; y++)
            {            
                cells.Add(Instantiate(cellPrefab, new Vector2(x, y), Quaternion.identity, areaPivot));
            }
        }

        pivotOffset = new Vector2(areaPivot.transform.position.x - sizeX / 2, areaPivot.transform.position.y - sizeY/ 2);
        areaPivot.transform.position = pivotOffset;
        _collider2d.offset = new Vector2((sizeX - 1) / 2f, (sizeY - 1) / 2f);
        _collider2d.size = new Vector2(sizeX, sizeY);
    }

    private void Redraw(Color color)
    {
        foreach (GameObject cell in cells)
        {
            cell.TryGetComponent(out SpriteRenderer sprite);
            sprite.color = color;
        }
    }

    public void SetPivotPosition(Vector2 _position)
    {
        areaPivot.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        //areaPivot.position = _position + pivotOffset;
    }
}
