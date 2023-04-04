using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private int teamID;
    [SerializeField] protected Color teamColor;
    [SerializeField] protected int score;

    public Area CurrentArea { get; set; }

    public int TeamID => teamID;
    public Color TeamColor => teamColor;
    public int Score => score;

    protected List<Transform> vacantCells;

    public void SetVacantCells(List<Transform> vacantCells)
    {
        this.vacantCells = vacantCells;
    }

    public virtual void SetMove(Area area)
    {
        CurrentArea = area;
        Debug.Log($"Player is moving : {transform.name}");
    }

    public virtual void AddScore(int ammount)
    {
        score += ammount;
    }

    private bool IsCanPlace(float _x, float _y, Area currentArea)
    {
        bool isPlaced = false;

        Vector2 startPosition = currentArea.transform.position;

        currentArea.transform.position = new Vector3(_x, _y, 0);
        currentArea.SetPivotPosition();

        for (int i = 0; i < 4; i++)
        {
            if (currentArea.IsCanPlace())
            {
                isPlaced = true;
                break;
            }
            else
            {
                currentArea.Rotate();
            }
        }

        currentArea.transform.position = startPosition;
        currentArea.SetPivotPosition();

        return isPlaced;
    }

    public bool IsAvaiableCells(Area area)
    {
        bool isAvaiableCells = false;

        for (int i = 0; i < vacantCells.Count; i++)
        {
            Transform cell = vacantCells[i];

            if (IsCanPlace(cell.position.x + 1, cell.position.y, area) ||
                IsCanPlace(cell.position.x - 1, cell.position.y, area) ||
                IsCanPlace(cell.position.x, cell.position.y + 1, area) ||
                IsCanPlace(cell.position.x, cell.position.y - 1, area))
            {
                isAvaiableCells = true;
                break;
            }
            else
            {
                isAvaiableCells = false;
            }
        }

        return isAvaiableCells;
    }
}
