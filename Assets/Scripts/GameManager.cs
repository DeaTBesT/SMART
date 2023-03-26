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

    private void Start()
    {
        Invoke(nameof(StartGame), isDebug ? 0 : 3);
        SetCorners();
    }

    private void StartGame()
    {
        Area newArea = GenerateArea();
        newArea.Controller = players[GetCurrentPlayer];
        players[GetCurrentPlayer].SetMove(newArea);
    }

    public void EndMove()
    {
        currentPlayer++;

        Area newArea = GenerateArea();
        newArea.Controller = players[GetCurrentPlayer];
        players[GetCurrentPlayer].SetMove(newArea);
    }

    private Area GenerateArea()
    {
        int sizeX = isDebug ? areaSizeX : Random.Range(1, 6);
        int sizeY = isDebug ? areaSizeY : Random.Range(1, 6);

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
}