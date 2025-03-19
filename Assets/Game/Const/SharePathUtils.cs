namespace Game
{
    public static class SharePathUtils
    {
        public static string GetGamePrefabView(string name)
        {
            return "Function/Prefab/" + name;
        }


        public static string GetSkinPath(CatPointType.PointType type)
        {
            switch (type)
            {
                case CatPointType.PointType.BUTTERFLY:
                    return "Function/Prefab/skin/butterfly";
                case CatPointType.PointType.FISH:
                    return "Function/Prefab/skin/fish";
                case CatPointType.PointType.DIAN:
                    return "Function/Prefab/skin/dian";
                default:
                    return "No Skin";
            }
        }

        public static class Audio
        {
            public const string BtnClick = "btnClick";
            public const string AudioEnd = "end";
            public const string AudioMiss = "miss";
            public const string AudioStart = "start";

            public static string GetAudioPath(string audioName)
            {
                return "Common/audio/" + audioName;
            }
        }

    }
}