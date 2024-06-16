using System.Collections.Generic;
using UnityEngine;

namespace Higo.UI
{
    public class UILayerData
    {
        public Transform Root;
        public List<UIPanelData> Panels = new();
        public int UUIDGenerator = 0;
    }
}