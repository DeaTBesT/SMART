using System.Collections.Generic;
using Gameplay;
using Managers;
using UnityEngine;

namespace Controllers
{
    public class EnemyController : Controller
    {
        public override void SetMove(Area area)
        {
            base.SetMove(area);
            TryPlaceCurrentArea();
        }

        public void TakeTurn()
        {
            var upgradeFirst = Random.Range(0, 2) == 0;
            if (upgradeFirst)
            {
                if (TryUpgradeRandomArea())
                {
                    return;
                }

                if (TryCreateArea())
                {
                    return;
                }
            }
            else
            {
                if (TryCreateArea())
                {
                    return;
                }

                if (TryUpgradeRandomArea())
                {
                    return;
                }
            }

            GameManager.Instance.EndCurrentTurn();
        }

        private bool TryCreateArea()
        {
            return GameManager.Instance.TryCreateAreaForCurrentPlayer();
        }

        private bool TryUpgradeRandomArea()
        {
            var areas = FindObjectsOfType<Area>();
            var ownedAreas = new List<Area>();

            foreach (var area in areas)
            {
                if (area.IsPlaced && area.Controller == this && area.CanUpgrade)
                {
                    ownedAreas.Add(area);
                }
            }

            if (ownedAreas.Count == 0)
            {
                return false;
            }

            var selectedArea = ownedAreas[UnityEngine.Random.Range(0, ownedAreas.Count)];
            selectedArea.Upgrade();
            GameManager.Instance.EndCurrentTurn();
            return true;
        }

        private void TryPlaceCurrentArea()
        {
            for (var i = 0; i < _vacantCells.Count; i++)
            {
                var cell = _vacantCells[i];

                if (TryPlaceAt(cell.position.x + 1, cell.position.y) ||
                    TryPlaceAt(cell.position.x - 1, cell.position.y) ||
                    TryPlaceAt(cell.position.x, cell.position.y + 1) ||
                    TryPlaceAt(cell.position.x, cell.position.y - 1))
                {
                    var currentArea = CurrentArea;
                    _vacantCells.AddRange(currentArea.Cells);
                    ///UpdateVacantCells();
                    break;
                }
            }
        }

        private bool TryPlaceAt(float x, float y)
        {
            // Snap target to nearest map cell; if none, don't attempt placement
            var target = new Vector2(x, y);
            if (!MapBuilder.Instance.TryGetNearestCell(target, out var nearestCell) || nearestCell == null)
            {
                return false;
            }

            var isPlaced = false;
            CurrentArea.transform.position = nearestCell.transform.position;
            CurrentArea.SetPivotPosition();

            for (var r = 0; r < 4; r++)
            {
                if (CurrentArea.PlacingArea())
                {
                    isPlaced = true;
                    break;
                }

                CurrentArea.Rotate();
            }

            return isPlaced;
        }
    }
}