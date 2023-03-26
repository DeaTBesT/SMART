using UnityEngine;

public class EnemyController : Controller
{
    public override void SetMove(Area area)
    {
        base.SetMove(area);

        CheckVacantCells();
    }

    private void CheckVacantCells()
    {
        for (int i = 0; i < vacantCells.Count; i++)
        {
            Transform cell = vacantCells[i];

            if (TryToPlace(cell.position.x + 1, cell.position.y) ||
                TryToPlace(cell.position.x - 1, cell.position.y) ||
                TryToPlace(cell.position.x, cell.position.y + 1) ||
                TryToPlace(cell.position.x, cell.position.y - 1))
            {
                vacantCells.RemoveAt(i);
                vacantCells.AddRange(CurrentArea.GetCells);

                break;
            }
        }
    }

    private bool TryToPlace(float _x, float _y)
    {
        bool isPlaced = false;

        CurrentArea.transform.position = new Vector3(_x, _y, 0);
        CurrentArea.SetPivotPosition();

        for (int i = 0; i < 4; i++)
        {
            if (CurrentArea.PlacingArea())
            {
                isPlaced = true;
                break;
            }
            else
            {
                isPlaced = false;
                CurrentArea.Rotate();
            }  
        }
    
        return isPlaced;
    }
}
