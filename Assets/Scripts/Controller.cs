using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] protected Color teamColor;

    public Area CurrentArea { get; set; }

    public Color TeamColor => teamColor;

    public virtual void SetMove()
    {
       
    }
}
