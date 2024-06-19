namespace Higo.UI
{
    public interface IUIPanelInit
    {
        void OnInit(PanelInfo uuid);
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
        void OnPause();
    }

    public interface IUIPanel : IUIPanelShow, IUIPanelHide, IUIPanelResume, IUIPanelPause
    {
    }
}