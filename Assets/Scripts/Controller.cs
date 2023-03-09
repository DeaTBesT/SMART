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

    public virtual void SetMove()
    {
       
    }

    public virtual void AddScore(int ammount)
    {
        score += ammount;
    }
}
