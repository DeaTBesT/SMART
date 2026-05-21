using System.Collections.Generic;
using System.Threading;
using Bootstrap;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay;
using UIModule;
using UnityEngine;

namespace Managers
{
    public class ResourceManager : Singleton<ResourceManager>, IInitializable
    {
        public event System.Action<int,int,int> OnChangeResources;

        [Header("UI")]
        [SerializeField] private ResourcePanel _resourcePanel;

        private readonly Dictionary<int, List<ResourceCell>> _capturedByTeam = new Dictionary<int, List<ResourceCell>>();
        private readonly Dictionary<int, int> _woodByTeam = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _oreByTeam = new Dictionary<int, int>();

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            UpdateUi();
            await UniTask.CompletedTask;
        }

        public void CaptureAreaResources(Area area)
        {
            if (area == null || area.Controller == null)
            {
                return;
            }

            var teamId = area.Controller.TeamID;

            if (!_capturedByTeam.TryGetValue(teamId, out var list))
            {
                list = new List<ResourceCell>();
                _capturedByTeam[teamId] = list;
            }

            foreach (var cell in area.Cells)
            {
                if (!MapBuilder.Instance.TryGetCellAtWorldPosition(cell.position, out var resourceCell))
                {
                    continue;
                }

                if (!resourceCell.CanCapture)
                {
                    continue;
                }

                resourceCell.Capture(teamId);
                list.Add(resourceCell);
            }

            UpdateUi();
        }

        public void CollectTurnResources(int teamId)
        {
            if (!_woodByTeam.ContainsKey(teamId))
            {
                _woodByTeam[teamId] = 0;
            }

            if (!_oreByTeam.ContainsKey(teamId))
            {
                _oreByTeam[teamId] = 0;
            }

            _woodByTeam[teamId] += 2;
            _oreByTeam[teamId] += 2;

            if (!_capturedByTeam.TryGetValue(teamId, out var list))
            {
                list = new List<ResourceCell>();
                _capturedByTeam[teamId] = list;
            }

            foreach (var resourceCell in list)
            {
                if (!resourceCell.IsCaptured)
                {
                    continue;
                }

                switch (resourceCell.ResourceType)
                {
                    case ResourceType.Wood:
                        _woodByTeam[teamId] += resourceCell.AmountPerTurn;
                        break;
                    case ResourceType.Ore:
                        _oreByTeam[teamId] += resourceCell.AmountPerTurn;
                        break;
                }
            }

            UpdateUi();
        }

        public int GetWoodForTeam(int teamId)
        {
            return _woodByTeam.TryGetValue(teamId, out var value) ? value : 0;
        }

        public int GetOreForTeam(int teamId)
        {
            return _oreByTeam.TryGetValue(teamId, out var value) ? value : 0;
        }

        public bool HasResources(int teamId, int woodCost, int oreCost)
        {
            return GetWoodForTeam(teamId) >= woodCost && GetOreForTeam(teamId) >= oreCost;
        }

        public bool TrySpendResources(int teamId, int woodCost, int oreCost)
        {
            if (!HasResources(teamId, woodCost, oreCost))
            {
                return false;
            }

            _woodByTeam[teamId] -= woodCost;
            _oreByTeam[teamId] -= oreCost;
            UpdateUi();
            return true;
        }

        public void UpdateUi()
        {
            // Raise change events for all known teams
            var teamIds = new System.Collections.Generic.HashSet<int>(_woodByTeam.Keys);
            foreach (var id in _oreByTeam.Keys) teamIds.Add(id);

            if (teamIds.Count > 0)
            {
                foreach (var id in teamIds)
                {
                    var wood = GetWoodForTeam(id);
                    var ore = GetOreForTeam(id);
                    OnChangeResources?.Invoke(id, wood, ore);
                }
            }

            // Also update configured panel directly (helps initial state)
            if (_resourcePanel != null)
            {
                var teamId = _resourcePanel.TeamId;
                _resourcePanel.SetResourceValues(GetWoodForTeam(teamId), GetOreForTeam(teamId));
            }
        }
    }
}
