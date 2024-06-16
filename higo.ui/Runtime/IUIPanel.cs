namespace Higo.UI
{
    public interface IUIPanelInit
    {
        void OnInit(UIUUID uuid);
    }

    public interface IUIPanelShow
    {
        void OnShow();
    }

    public interface IUIPanelHide
    {
        void OnHide();
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