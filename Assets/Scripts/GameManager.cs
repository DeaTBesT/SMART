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

    private void Start()
    {
        Invoke(nameof(StartGame), isDebug ? 0 : 3);
    }

    private void StartGame()
    {
        GenerateArea().Controller = players[isDebug ? 0 : 1];
    }

    public void EndMove()
    {
        
    }

    private Area GenerateArea()
    {
        int sizeX = isDebug ? areaSizeX : Random.Range(1, 6);
        int sizeY = isDebug ? areaSizeY : Random.Range(1, 6);

        Instantiate(areaPrefab.gameObject, Vector2.zero, Quaternion.identity).TryGetComponent(out Area newArea);
        newArea.GenerateArea(cellPrefab, sizeX, sizeY);

        return newArea;
    }
}