using System;
using Game.Main.Cat;

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
                case CatPointType.PointType.FISH:
                    return "Assets/Function/Prefab/skin/fish";
                case CatPointType.PointType.BALL:
                    return "Assets/Function/Prefab/skin/ball";
                case CatPointType.PointType.DIAN:
                    return "Assets/Function/Prefab/skin/dian";
                default:
                    return "No Skin";
            }
        }
    }
}