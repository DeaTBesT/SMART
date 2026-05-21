using System;
using System.Threading;
using Bootstrap;
using Controllers;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay;
using Services;
using UnityEngine;

namespace Managers
{
    public class GameManager : Singleton<GameManager>, IInitializable
    {
        [SerializeField] private Controller[] _players;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Area _areaPrefab;

        [Header("Debug")]
        [SerializeField] private bool _isDebug;
        [SerializeField] private int _areaSizeX;
        [SerializeField] private int _areaSizeY;

        private readonly AreaFactory _areaFactory = new AreaFactory();
        private int _currentPlayer;
        private Area[,] _vacantCells;

        public int Players => _players.Length;
        private int CurrentPlayerIndex => (_currentPlayer + _players.Length) % _players.Length;

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            _vacantCells = new Area[MapBuilder.Instance.MapSizeX, MapBuilder.Instance.MapSizeY];
            SetCorners();
            await StartGameAsync(cancellationToken);
        }

        private async UniTask StartGameAsync(CancellationToken cancellationToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_isDebug ? 0 : 3), cancellationToken: cancellationToken);
            BeginTurn();
        }

        private void BeginTurn()
        {
            ResourceManager.Instance?.CollectTurnResources();

            var newArea = CreateAreaForCurrentPlayer();
            newArea.Controller = _players[CurrentPlayerIndex];
            _players[CurrentPlayerIndex].SetMove(newArea);
        }

        public void EndMove(Area area)
        {
            FillVacantCells(area);
            _currentPlayer++;

            var newArea = CreateAreaForCurrentPlayer();
            newArea.Controller = _players[CurrentPlayerIndex];

            if (_players[CurrentPlayerIndex].HasAvailableMove(newArea))
            {
                _players[CurrentPlayerIndex].SetMove(newArea);
                ResourceManager.Instance?.CollectTurnResources();
            }
            else
            {
                EndGame(_players[CurrentPlayerIndex]);
            }
        }

        public void EndGame(Controller controller)
        {
            Debug.Log($"{controller.name} : End game");
        }

        private Area CreateAreaForCurrentPlayer()
        {
            var width = _isDebug ? _areaSizeX : UnityEngine.Random.Range(1, 6);
            var height = _isDebug ? _areaSizeY : UnityEngine.Random.Range(1, 6);

            return _areaFactory.CreateArea(_areaPrefab, _cellPrefab, width, height);
        }

        private void SetCorners()
        {
            for (var i = 0; i < Players; i++)
            {
                var currentCorner = GetCornerIndex(i);
                var selectedArea = MapBuilder.Instance.Corners[currentCorner];

                selectedArea.areaCorner.Controller = _players[i];
                selectedArea.areaCorner.AreaCollider.SetActiveArea(true);
                selectedArea.areaCorner.AreaCollider.gameObject.layer = 6;
                _players[i].SetVacantCells(selectedArea.vacantCells);
            }
        }

        private int GetCornerIndex(int playerIndex)
        {
            if (Players == 2)
            {
                return playerIndex == 0 ? 0 : 3;
            }

            return playerIndex % MapBuilder.Instance.Corners.Length;
        }

        private void FillVacantCells(Area area)
        {
            for (var x = (int)area.StartPoint.x; x <= (int)area.EndPoint.x; x++)
            {
                for (var y = (int)area.StartPoint.y; y <= (int)area.EndPoint.y; y++)
                {
                    var positionX = ((int)area.Cells[0].position.x + MapBuilder.Instance.MapSizeX / 2) + x;
                    var positionY = ((int)area.Cells[0].position.y + MapBuilder.Instance.MapSizeY / 2) + y;

                    _vacantCells[positionX, positionY] = area;
                }
            }
        }

        public bool CheckVacantCell(Transform cell)
        {
            var cellPositionX = (int)cell.position.x + MapBuilder.Instance.MapSizeX / 2;
            var cellPositionY = (int)cell.position.y + MapBuilder.Instance.MapSizeY / 2;

            var freeNeighborCount = 0;

            if (IsInBounds(cellPositionX + 1, cellPositionY) && _vacantCells[cellPositionX + 1, cellPositionY] == null)
            {
                freeNeighborCount++;
            }

            if (IsInBounds(cellPositionX - 1, cellPositionY) && _vacantCells[cellPositionX - 1, cellPositionY] == null)
            {
                freeNeighborCount++;
            }

            if (IsInBounds(cellPositionX, cellPositionY + 1) && _vacantCells[cellPositionX, cellPositionY + 1] == null)
            {
                freeNeighborCount++;
            }

            if (IsInBounds(cellPositionX, cellPositionY - 1) && _vacantCells[cellPositionX, cellPositionY - 1] == null)
            {
                freeNeighborCount++;
            }

            return freeNeighborCount > 0;
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < _vacantCells.GetLength(0) && y >= 0 && y < _vacantCells.GetLength(1);
        }
    }
}
