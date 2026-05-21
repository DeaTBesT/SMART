using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace Services
{
    public static class PlacementValidator
    {
        public static bool HasAvailableMove(Area area, IReadOnlyList<Transform> vacantCells)
        {
            if (area == null)
            {
                return false;
            }

            for (var i = 0; i < vacantCells.Count; i++)
            {
                var cell = vacantCells[i];

                if (CanPlaceAt(cell.position.x + 1, cell.position.y, area) ||
                    CanPlaceAt(cell.position.x - 1, cell.position.y, area) ||
                    CanPlaceAt(cell.position.x, cell.position.y + 1, area) ||
                    CanPlaceAt(cell.position.x, cell.position.y - 1, area))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CanPlaceAt(float x, float y, Area area)
        {
            var startPosition = area.transform.position;
            area.transform.position = new Vector3(x, y, 0f);
            area.SetPivotPosition();

            var isPlaced = false;
            for (var i = 0; i < 4; i++)
            {
                if (area.IsCanPlace())
                {
                    isPlaced = true;
                    break;
                }
                area.Rotate();
            }

            area.transform.position = startPosition;
            area.SetPivotPosition();

            return isPlaced;
        }
    }
}
