using UnityEngine;
using static MapBuilder;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Controller[] players;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Area areaPrefab;

    [Header("Debug")]
    [SerializeField] private bool isDebug;
    [SerializeField] private int areaSizeX;
    [SerializeField] private int areaSizeY;

    private int currentPlayer;
    private int GetCurrentPlayer
    {
        get
        {
            return (currentPlayer + players.Length) % players.Length;
        }
    }

    public int Players => players.Length;

    private Area[,] vacantCells;

    private void Start()
    {
        vacantCells = new Area[MapBuilder.Instance.MapSizeX, MapBuilder.Instance.MapSizeY];

        Invoke(nameof(StartGame), isDebug ? 0 : 3);
        SetCorners();
    }

    private void StartGame()
    {
        Area newArea = GenerateArea(isDebug ? areaSizeX : Random.Range(1, 6), isDebug ? areaSizeY : Random.Range(1, 6));
        newArea.Controller = players[GetCurrentPlayer];
        players[GetCurrentPlayer].SetMove(newArea);
    }

    public void EndMove(Area area)
    {
        for (int x = (int)area.GetStartPoint.x; x <= (int)area.GetEndPoint.x; x++)
        {
            for (int y = (int)area.GetStartPoint.y; y <= (int)area.GetEndPoint.y; y++)
            {
                int positionX = ((int)area.GetCells[0].transform.position.x + MapBuilder.Instance.MapSizeX / 2) + x;
                int positionY = ((int)area.GetCells[0].transform.position.y + MapBuilder.Instance.MapSizeY / 2) + y;

                vacantCells[positionX, positionY] = area;
            }
        }

        currentPlayer++;

        Area newArea = GenerateArea(isDebug ? areaSizeX : Random.Range(1, 6), isDebug ? areaSizeY : Random.Range(1, 6));
        newArea.Controller = players[GetCurrentPlayer];

        if (players[GetCurrentPlayer].IsAvaiableCells(newArea))
        {
            players[GetCurrentPlayer].SetMove(newArea);
        }
        else
        {
            EndGame(players[GetCurrentPlayer]);
        }
    }

    public void EndGame(Controller controller)
    {
        Debug.Log($"{controller.name} : End game");
    }

    private Area GenerateArea(int sizeX, int sizeY)
    {
        Instantiate(areaPrefab.gameObject, Vector2.zero, Quaternion.identity).TryGetComponent(out Area newArea);
        newArea.GenerateArea(cellPrefab, sizeX, sizeY);
        newArea.m_AreaCollider.SetActiveArea(false);

        return newArea;
    }

    private void SetCorners()
    {
        for (int i = 0; i < Players; i++)
        {
            int currentCorner = (i + Players) % Players + 1;

            Corner selectedArea = MapBuilder.Instance.Corners[currentCorner];
            selectedArea.areaCorner.Controller = players[i];
            selectedArea.areaCorner.m_AreaCollider.SetActiveArea(true);
            selectedArea.areaCorner.m_AreaCollider.gameObject.layer = 6;
            players[i].SetVacantCells(selectedArea.vacantCells);
        }
    }

    public bool CheckVacantCell(Transform cell)
    {
        int cellPositionX = (int)cell.transform.position.x + MapBuilder.Instance.MapSizeX / 2;
        int cellPositionY = (int)cell.transform.position.y + MapBuilder.Instance.MapSizeY / 2;

        int isVacant = 0;

        if (((cellPositionX + 1 < vacantCells.GetLength(0)) && (cellPositionY < vacantCells.GetLength(1)) && (cellPositionY >= 0)) &&
            (vacantCells[cellPositionX + 1, cellPositionY] == null)) { isVacant++; }

        if (((cellPositionX - 1 >= 0) && (cellPositionY < vacantCells.GetLength(1)) && (cellPositionY >= 0)) &&
            (vacantCells[cellPositionX - 1, cellPositionY] == null)) { isVacant++; }

        if (((cellPositionY + 1 < vacantCells.GetLength(1)) && (cellPositionX < vacantCells.GetLength(0)) && (cellPositionX >= 0)) &&
            (vacantCells[cellPositionX, cellPositionY + 1] == null)) { isVacant++; }

        if (((cellPositionY - 1 >= 0) && (cellPositionX < vacantCells.GetLength(0)) && (cellPositionX >= 0)) &&
            (vacantCells[cellPositionX, cellPositionY - 1] == null)) { isVacant++; }

        return isVacant <= 0 ? false : true;
    }
}