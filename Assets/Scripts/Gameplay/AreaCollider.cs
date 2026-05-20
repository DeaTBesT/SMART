using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AreaCollider : MonoBehaviour
{
    [SerializeField] private int _startLayer;
    [SerializeField] private int _placementLayer;
    [SerializeField] private Area _parentArea;
    private BoxCollider2D _boxCollider;

    public Area Area => _parentArea;

    private void OnEnable()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _boxCollider.isTrigger = true;
        gameObject.layer = _startLayer;

        if (_parentArea == null)
        {
            _parentArea = GetComponentInParent<Area>();
        }

        _parentArea.AreaCollider = this;
    }

    public void SetActiveArea(bool value)
    {
        gameObject.layer = value ? _placementLayer : _startLayer;
    }

    public void SetArea(Area area)
    {
        _parentArea = area;
    }
}
