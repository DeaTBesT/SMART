using System;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class AreaFactory
{
    public Area CreateArea(Area areaPrefab, GameObject cellPrefab, int width, int height)
    {
        if (areaPrefab == null)
        {
            throw new ArgumentNullException(nameof(areaPrefab));
        }

        if (cellPrefab == null)
        {
            throw new ArgumentNullException(nameof(cellPrefab));
        }

        var areaObject = Object.Instantiate(areaPrefab.gameObject, Vector2.zero, Quaternion.identity);
        if (!areaObject.TryGetComponent(out Area area))
        {
            throw new InvalidOperationException("Area prefab must contain an Area component.");
        }

        area.GenerateArea(cellPrefab, width, height);
        area.AreaCollider.SetActiveArea(false);

        return area;
    }
}
