namespace Game
{
    public static class CatConst
    {
        // public const string LoadingView => SharePathUtils.GetGamePrefabView("choosePage");


        public static string LoadingView => SharePathUtils.GetGamePrefabView("loadingPage");
        public static string FirstPageView => SharePathUtils.GetGamePrefabView("firstPage");
        public static string MainPageView => SharePathUtils.GetGamePrefabView("mainPage");
        public static string SettingView => SharePathUtils.GetGamePrefabView("settingPanel");
    }
}