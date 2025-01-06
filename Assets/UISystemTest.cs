using Higo.UI;
using UnityEngine;
using UnityEngine.Assertions;

public enum UILayers
{
    Base,
    Dialog,
    Overlap,
}

public class UISystemTest : MonoBehaviour
{
    void Awake()
    {
        var uiSys = UISystem.Instance;
        var auuid = uiSys.Show(UILayers.Base, "a", true);
        var buuid = uiSys.Show(UILayers.Base, "b", false);
        var cuuid = uiSys.Show(UILayers.Base, "c", true);
        var duuid = uiSys.Show(UILayers.Base, "d", false);
        var euuid = uiSys.Show(UILayers.Base, "e", true);

        uiSys.Hide(buuid);
        uiSys.Hide(auuid);
        uiSys.Hide(euuid);
        uiSys.Hide(duuid);
        uiSys.Hide(UILayers.Base, "c");

        uiSys.PathPrefix = "UI";
        uiSys.Show(UILayers.Base, "MainUI", false);
    }
}