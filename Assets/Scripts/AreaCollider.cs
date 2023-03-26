using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AreaCollider : MonoBehaviour
{
    [SerializeField] private int startLayer;
    [SerializeField] private int placementLayer;
    [SerializeField] private Area parentArea;
    private BoxCollider2D boxCollider;

    public Area GetArea => parentArea;

    private void OnEnable()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        gameObject.layer = startLayer;

        if (parentArea == null)
        {
            parentArea = GetComponentInParent<Area>();
        }

        parentArea.m_AreaCollider = this;
    }

    public void SetActiveArea(bool _value)
    {
        gameObject.layer = _value ? placementLayer : startLayer;
    }

    public void SetArea(Area area)
    {
        parentArea = area;
    }
}
