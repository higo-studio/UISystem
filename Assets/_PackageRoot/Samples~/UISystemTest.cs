using UnityEngine;
using UnityEngine.Assertions;

public class UISystemTest : MonoBehaviour
{
    void Awake()
    {
        var uiSys = UISystem.current;
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