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

        public static class Audio
        {
            public const string BtnClick = "btnClick.mp3";
            public const string AudioEnd = "end.mp3";
            public const string AudioMiss = "miss.mp3";
            public const string AudioStart = "start.mp3";

            public static string GetAudioPath(string audioName)
            {
                return "Assets/Common/audio/" + audioName;
            }
        }

    }
}