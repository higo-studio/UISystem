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
        Debug.Log(UISystem.Instance.Show(UILayers.Dialog, "PopupUI"));
    }
}
