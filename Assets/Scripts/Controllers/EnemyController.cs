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
                    _vacantCells.AddRange(CurrentArea.Cells);
                    UpdateVacantCells();
                    break;
                }
            }
        }

        private bool TryPlaceAt(float x, float y)
        {
            var isPlaced = false;
            CurrentArea.transform.position = new Vector3(x, y, 0f);
            CurrentArea.SetPivotPosition();

            for (var i = 0; i < 4; i++)
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

        private void UpdateVacantCells()
        {
            _vacantCells.RemoveAll(cell => !GameManager.Instance.CheckVacantCell(cell));
        }
    }
}
