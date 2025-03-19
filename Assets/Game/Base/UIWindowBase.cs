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

        protected UIWindowBase(uint windowId, string windowName)
        {
            WindowId = windowId;
            this.windowName = windowName;
        }

        public override void Show(params object[] arg)
        {
            if (DisplayObject == null)
            {
                var gameObject = ResourceLoader.Instance.CatLoadPrefab(windowName);
                SetDisplayObject(gameObject);
            }
            base.Show(arg);
        }


        public virtual bool Stack()
        {
            return true;
        }
    }
}