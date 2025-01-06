using System.Collections.Generic;
using UnityEngine;

namespace Higo.UI
{
    public class UILayerData : IReadOnlyUILayerData
    {
        public Transform Root { get; set; }
        public List<UIPanelData> Panels = new();
        public int UUIDGenerator { get; set; } = 0;

        IReadOnlyList<UIPanelData> IReadOnlyUILayerData.Panels => Panels;
    }

    public interface IReadOnlyUILayerData
    {
        Transform Root { get; }
        IReadOnlyList<UIPanelData> Panels { get; }
        int UUIDGenerator { get; }
    }
}