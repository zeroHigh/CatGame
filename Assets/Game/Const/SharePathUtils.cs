namespace Game
{
    public static class SharePathUtils
    {
        public static string GetGamePrefabView(string name)
        {
            return "Assets/Function/Prefab/" + name;
        }


        public static string GetSkinPath(CatPointType.PointType type)
        {
            switch (type)
            {
                case CatPointType.PointType.BUTTERFLY:
                    return "Assets/Function/Prefab/skin/butterfly";
                case CatPointType.PointType.FISH:
                    return "Assets/Function/Prefab/skin/fish";
                case CatPointType.PointType.DIAN:
                    return "Assets/Function/Prefab/skin/dian";
                default:
                    return "No Skin";
            }
        }
    }
}