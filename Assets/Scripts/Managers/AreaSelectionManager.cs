using System;
using Core;
using Gameplay;

namespace Managers
{
    public class AreaSelectionManager : Singleton<AreaSelectionManager>
    {
        public event Action<Area> OnSelectedAreaChanged;

        public Area SelectedArea { get; private set; }

        public void SelectArea(Area area)
        {
            if (SelectedArea == area)
            {
                return;
            }

            SelectedArea = area;
            OnSelectedAreaChanged?.Invoke(SelectedArea);
        }

        public void ClearSelection()
        {
            if (SelectedArea == null)
            {
                return;
            }

            SelectedArea = null;
            OnSelectedAreaChanged?.Invoke(null);
        }
    }
}
