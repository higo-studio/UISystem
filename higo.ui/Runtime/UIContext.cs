namespace Higo.UI
{
    public struct PanelInfo
    {
        public UIUUID Uuid;
        public string Path;
    }

    public struct UIHideContext
    {
        public bool DontDestroy;
    }

    public struct UIPauseContext
    {
        // public bool DontDestroy;
    }
}