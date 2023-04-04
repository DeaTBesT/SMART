using System.Collections.Generic;
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

            if (TryToPlace(cell.position.x + 1, cell.position.y, CurrentArea) ||
                TryToPlace(cell.position.x - 1, cell.position.y, CurrentArea) ||
                TryToPlace(cell.position.x, cell.position.y + 1, CurrentArea) ||
                TryToPlace(cell.position.x, cell.position.y - 1, CurrentArea))
            {
                vacantCells.AddRange(CurrentArea.GetCells);
                CheckRemoveabelCells();
                break;
            }
        }
    }

    private bool TryToPlace(float _x, float _y, Area currentArea)
    {
        bool isPlaced = false;

        currentArea.transform.position = new Vector3(_x, _y, 0);
        currentArea.SetPivotPosition();

        for (int i = 0; i < 4; i++)
        {
            if (currentArea.PlacingArea())
            {
                isPlaced = true;
                break;
            }
            else
            {
                currentArea.Rotate();
            }  
        }
    
        return isPlaced;
    }

    private void CheckRemoveabelCells()
    {
        List<Transform> removeableCells = new List<Transform>();

        for (int i = 0; i < vacantCells.Count; i++)
        {
            Transform cell = vacantCells[i];

            if (!GameManager.Instance.CheckVacantCell(cell))
            {
                removeableCells.Add(cell);
            }
        }

        foreach (Transform _cell in removeableCells)
        {
            vacantCells.Remove(_cell);
        }
    }
}
