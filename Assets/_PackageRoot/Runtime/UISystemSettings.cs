using Higo.UI;
using UnityEngine;

namespace Higo.UI
{
    [CreateAssetMenu]
    public class UISystemSettings : ScriptableObject
    {
        public readonly static string Path = $"Asset/Settings/Resources/{nameof(UISystemSettings)}.asset";
        public UISystem Prefab;

        public void Reset()
        {
            Prefab = Resources.Load<UISystem>("Packages/higo.ui-system/Resources/UI System.prefab");
        }
    }
}