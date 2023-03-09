using UnityEngine;

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
        GenerateArea().Controller = players[GetCurrentPlayer];
    }

    public void EndMove()
    {
        currentPlayer++;
        GenerateArea().Controller = players[GetCurrentPlayer];
    }

    private Area GenerateArea()
    {
        int sizeX = isDebug ? areaSizeX : Random.Range(1, 6);
        int sizeY = isDebug ? areaSizeY : Random.Range(1, 6);

        Instantiate(areaPrefab.gameObject, Vector2.zero, Quaternion.identity).TryGetComponent(out Area newArea);
        newArea.GenerateArea(cellPrefab, sizeX, sizeY);

        return newArea;
    }

    private void SetCorners()
    {
        for (int i = 0; i < Players; i++)
        {
            int currentCorner = (i + Players) % Players + 1;

            MapBuilder.Instance.Corners[currentCorner].areaCorner.Controller = players[i];
        }
    }
}