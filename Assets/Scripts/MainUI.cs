using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Higo.UI;

public class MainUI : MonoBehaviour, IUIPanelShow
{
    public void OnShow()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPopup()
    {
        UISystem.Instance.OpenUI(UILayers.Dialog, "PopupUI");
    }
}
