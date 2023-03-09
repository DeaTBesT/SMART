using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AreaCollider : MonoBehaviour
{
    [SerializeField] private Area parentArea;

    public Area GetArea => parentArea;

    private void Start()
    {
        if (parentArea == null)
        {
            parentArea = GetComponentInParent<Area>();
        }
    }

    public void SetArea(Area area)
    {
        parentArea = area;
    }
}
