namespace Higo.UI
{
    public struct UIPanelData
    {
        public enum States
        {
            Hided, Shown, Freezed
        }

        public string Path;
        public States State;
        public bool IsExclusive;
        public Uuid UUID;
    }
}