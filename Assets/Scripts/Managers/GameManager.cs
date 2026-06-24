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
        public event Action<bool> OnPlayerTurnChanged;
        [SerializeField] private Controller[] _players;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Area _areaPrefab;

        [Header("Debug")] [SerializeField] private bool _isDebug;
        [SerializeField] private int _areaSizeX;
        [SerializeField] private int _areaSizeY;

        private readonly AreaFactory _areaFactory = new AreaFactory();
        private int _currentPlayer;
        private Area[,] _vacantCells;

        public int Players => _players.Length;
        public Controller CurrentPlayer => _players[CurrentPlayerIndex];
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
            var currentPlayer = _players[CurrentPlayerIndex];
            currentPlayer.CurrentArea = null;
            AreaSelectionManager.Instance?.ClearSelection();
            ResourceManager.Instance?.CollectTurnResources(currentPlayer.TeamID);
            OnPlayerTurnChanged?.Invoke(currentPlayer is PlayerController);

            if (currentPlayer is EnemyController enemy)
            {
                enemy.TakeTurn();
            }
        }

        public void EndMove(Area area)
        {
            FillVacantCells(area);
            _currentPlayer++;
            BeginTurn();
        }

        public void EndGame(Controller controller)
        {
            Debug.Log($"{controller.name} : End game");
        }

        private Area CreateAreaForCurrentPlayer()
        {
            var width = _isDebug ? _areaSizeX : UnityEngine.Random.Range(1, 3);
            var height = _isDebug ? _areaSizeY : UnityEngine.Random.Range(1, 3);

            return _areaFactory.CreateArea(_areaPrefab, _cellPrefab, width, height);
        }

        public bool CanCreateAreaForCurrentPlayer()
        {
            var currentPlayer = _players[CurrentPlayerIndex];
            return currentPlayer is PlayerController && currentPlayer.CurrentArea == null &&
                   ResourceManager.Instance != null &&
                   ResourceManager.Instance.HasResources(currentPlayer.TeamID, 2, 2);
        }

        public bool TryCreateAreaForCurrentPlayer()
        {
            var currentPlayer = _players[CurrentPlayerIndex];
            if (currentPlayer.CurrentArea != null)
            {
                return false;
            }

            if (ResourceManager.Instance == null ||
                !ResourceManager.Instance.TrySpendResources(currentPlayer.TeamID, 2, 2))
            {
                return false;
            }

            AreaSelectionManager.Instance?.ClearSelection();
            var newArea = CreateAreaForCurrentPlayer();
            newArea.Controller = currentPlayer;
            currentPlayer.SetMove(newArea);
            return true;
        }

        public bool CanUpgradeSelectedArea()
        {
            var selectedArea = AreaSelectionManager.Instance?.SelectedArea;
            if (selectedArea == null || selectedArea.Controller != CurrentPlayer || !selectedArea.CanUpgrade)
            {
                return false;
            }

            var upgradeCost = CalculateUpgradeCost(selectedArea);
            return ResourceManager.Instance != null &&
                   ResourceManager.Instance.HasResources(CurrentPlayer.TeamID, upgradeCost.wood, upgradeCost.ore);
        }

        public bool TryUpgradeSelectedArea()
        {
            var selectedArea = AreaSelectionManager.Instance?.SelectedArea;
            if (selectedArea == null || selectedArea.Controller != CurrentPlayer)
            {
                return false;
            }

            var upgradeCost = CalculateUpgradeCost(selectedArea);
            if (ResourceManager.Instance == null ||
                !ResourceManager.Instance.TrySpendResources(CurrentPlayer.TeamID, upgradeCost.wood, upgradeCost.ore))
            {
                return false;
            }

            selectedArea.Upgrade();
            AreaSelectionManager.Instance?.ClearSelection();
            EndCurrentTurn();
            return true;
        }

        private (int wood, int ore) CalculateUpgradeCost(Area area)
        {
            var cellCount = area.Cells.Count;
            var woodCost = cellCount;
            var oreCost = cellCount;
            return (woodCost, oreCost);
        }

        public void OnCreateAreaButtonPressed()
        {
            TryCreateAreaForCurrentPlayer();
        }

        public void OnUpgradeAreaButtonPressed()
        {
            TryUpgradeSelectedArea();
        }

        public void EndCurrentTurn()
        {
            _currentPlayer++;
            BeginTurn();
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
                return playerIndex == 0 ? 0 : 1;
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