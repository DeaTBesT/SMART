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
}
