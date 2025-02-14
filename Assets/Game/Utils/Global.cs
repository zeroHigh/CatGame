using UnityEngine;

namespace Game
{
    public class Global
    {
        public static int UserId;

        protected Global()
        {
            Logger.Log("Global");
        }

        public static bool IsMobile
        {
            get
            {
                return Application.platform == RuntimePlatform.Android ||
                       Application.platform == RuntimePlatform.IPhonePlayer;
            }
        }
    }
}