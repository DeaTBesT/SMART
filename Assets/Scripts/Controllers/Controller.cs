using System.Collections.Generic;
using Gameplay;
using Services;
using UnityEngine;

namespace Controllers
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private int _teamId;
        [SerializeField] protected Color _teamColor;
        [SerializeField] protected int _score;

        public Area CurrentArea { get; set; }

        public int TeamID => _teamId;
        public Color TeamColor => _teamColor;
        public int Score => _score;
        public static event System.Action OnScoreChanged;

        protected List<Transform> _vacantCells = new List<Transform>();

        public void SetVacantCells(List<Transform> vacantCells)
        {
            _vacantCells = vacantCells ?? new List<Transform>();
        }

        public List<Transform> GetVacantCells()
        {
            return _vacantCells;
        }

        public virtual void SetMove(Area area)
        {
            CurrentArea = area;
            Debug.Log($"Player is moving : {transform.name}");
        }

        public virtual void AddScore(int amount)
        {
            _score += amount;
            OnScoreChanged?.Invoke();
        }

        public bool HasAvailableMove(Area area)
        {
            if (area == null || _vacantCells == null)
            {
                return false;
            }

            return PlacementValidator.HasAvailableMove(area, _vacantCells);
        }
    }
}

