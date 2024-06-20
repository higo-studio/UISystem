using System.Collections.Generic;

namespace Higo.UI
{
    public interface IUIPanelInit
    {
        void OnInit(PanelInfo info);
    }

    public interface IUIPanelShow
    {
        void OnShow();
    }

    public interface IUIPanelHide
    {
        void OnHide(ref UIHideContext ctx);
    }

    public interface IUIPanelResume
    {
        void OnResume();
    }

    public interface IUIPanelPause
    {
        void OnPause(ref UIPauseContext ctx);
    }

    public interface IUIPanel : IUIPanelShow, IUIPanelHide, IUIPanelResume, IUIPanelPause
    {
    }

    public interface IUILayer
    {
        void Init(int layerIndex);
        void OnPanelShow(IReadOnlyList<PanelInfo> panels);
        void OnPanelHide(IReadOnlyList<PanelInfo> panels);
        void OnPanelPause(IReadOnlyList<PanelInfo> panels);
        void OnPanelResume(IReadOnlyList<PanelInfo> panels);
    }
}