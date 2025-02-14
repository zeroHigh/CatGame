namespace Game
{
    public enum WindowLayer
    {
        Bottom,
        Middle,
        Top,
        Guide,
    }

    public abstract class UIWindowBase : UIBaseView
    {
        private readonly string windowName;
        public WindowLayer Layer { get; protected set; }
        public uint WindowId { get; }
        protected string windowPrepareABName;

        protected UIWindowBase(uint windowId, string windowName)
        {
            windowPrepareABName = "";
            WindowId = windowId;
            this.windowName = windowName;
        }

        public override void Show(params object[] arg)
        {
            if (DisplayObject == null)
            {
                PrepareWindowAssetBundle();
                var gameObject = ResourceLoader.Instance.LoadObject(windowName);
                SetDisplayObject(gameObject);
            }
            base.Show(arg);
        }

        protected void PrepareWindowAssetBundle()
        {
            if (!string.IsNullOrEmpty(windowPrepareABName))
                ResourceLoader.Instance.PrepareBundle(windowPrepareABName);
        }

        protected void UnPrepareWindowAssetBundle()
        {
            if (!string.IsNullOrEmpty(windowPrepareABName))
                ResourceLoader.Instance.UnloadPreBundle(windowPrepareABName);
        }

        public override void Dispose()
        {
            UnPrepareWindowAssetBundle();
            base.Dispose();
        }

        public virtual bool Stack()
        {
            return true;
        }
    }
}