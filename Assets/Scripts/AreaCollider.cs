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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out AreaCollider area))
        {
            parentArea.IsTriggered(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out AreaCollider area))
        {
            parentArea.IsTriggered(false);
        }
    }
}
