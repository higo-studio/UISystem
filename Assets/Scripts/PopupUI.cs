using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Higo.UI;

public class PopupUI : MonoBehaviour, IUIPanelShow
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

    public void Close()
    {
        UISystem.Instance.Hide(UILayers.Dialog);
    }
}
