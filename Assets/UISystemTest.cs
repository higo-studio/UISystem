using Higo.UI;
using UnityEngine;
using UnityEngine.Assertions;

public enum UILayers
{
    Base
}

public static class UISystemExtension
{
    public static UIUUID OpenUI(this UISystem @this, UILayers layer, string name, bool isExclusive = true)
        => @this.OpenUI((int)layer, name, isExclusive);
    public static void CloseUI(this UISystem @this, UILayers layer, string name)
        => @this.CloseUI((int)layer, name);
}

public class UISystemTest : MonoBehaviour
{
    void Awake()
    {
        var uiSys = UISystem.Instance;
        var auuid = uiSys.OpenUI(UILayers.Base, "a", true);
        var buuid = uiSys.OpenUI(UILayers.Base, "b", false);
        var cuuid = uiSys.OpenUI(UILayers.Base, "c", true);
        var duuid = uiSys.OpenUI(UILayers.Base, "d", false);
        var euuid = uiSys.OpenUI(UILayers.Base, "e", true);

        uiSys.CloseUI(buuid);
        uiSys.CloseUI(auuid);
        uiSys.CloseUI(euuid);
        uiSys.CloseUI(duuid);
        uiSys.CloseUI(UILayers.Base, "c");
    }
}