using System.Collections.Generic;
using Higo.UI;
using UnityEngine;
using UnityEngine.UI;

public class SimpleUILayer : MonoBehaviour, IUILayer
{
    protected int m_LayerIndex;
    public Button Mask;
    public IReadOnlyUILayerData Layer => UISystem.Instance.GetLayer(m_LayerIndex);

    protected void OnEnable()
    {
        Mask.onClick.AddListener(OnMaskClick);
    }

    protected void OnDisable()
    {
        Mask.onClick.RemoveListener(OnMaskClick);
    }

    protected void OnMaskClick()
    {
        UISystem.Instance.CloseUI(m_LayerIndex);
    }

    public void Init(int layerIndex)
    {
        m_LayerIndex = layerIndex;
        SetMaskEnabled(Layer.Panels.Count > 0);
    }

    public void OnPanelShow(IReadOnlyList<PanelInfo> panels)
    {
        SetMaskEnabled(true);
    }

    public void OnPanelHide(IReadOnlyList<PanelInfo> panels)
    {
        SetMaskEnabled(Layer.Panels.Count > 0);
    }

    public void OnPanelPause(IReadOnlyList<PanelInfo> panels)
    {
    }

    public void OnPanelResume(IReadOnlyList<PanelInfo> panels)
    {
    }

    public void SetMaskEnabled(bool enabled)
    {
        if (Mask.gameObject.activeSelf != enabled)
        {
            Mask.gameObject.SetActive(enabled);
        }
    }
}