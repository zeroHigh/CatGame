using System;
using Game.Main.Cat;

namespace Game
{
    public class CatMainView : UIBaseView
    {
        private CatPointType.PointType _pointType;

        protected override void ParseComponent()
        {

        }

        protected override void Refresh(params object[] arg)
        {
            _pointType = (CatPointType.PointType)arg[0];
            var pointPath = "";
            switch (_pointType)
            {
                case CatPointType.PointType.FISH:
                    pointPath = "";
                    break;
                case CatPointType.PointType.BALL:
                    break;
                case CatPointType.PointType.DIAN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }



        }
    }
}