using System;
using System.Collections.Generic;

namespace Game
{
    public class ILSingleton<T> where T : ILSingleton<T>, new()
    {
        protected static T _instance;
        private static readonly object sysLock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (sysLock)
                    {
                        _instance = Activator.CreateInstance<T>();
                        ILSingLetonManager.RegModel<T>(_instance.Dispose);
                    }
                }
                return _instance;
            }
        }

        public virtual void Dispose()
        {
            _instance = null;
        }
    }

    public class ILSingLetonManager
    {
        /// <summary> 所有需要清理的单例类列表 的清理函数</summary>
        private static List<Action> _allList = new List<Action>(10);

        protected ILSingLetonManager()
        {
            Logger.Log("ILSingLetonManager");
        }

        /// <summary>
        /// 注册单例类的回收
        /// </summary>
        /// <param name="model"></param>
        public static void RegModel<T>(Action dispose) where T : ILSingleton<T>, new()
        {
            _allList.Add(dispose);
        }

        /// <summary>
        /// 清理所有单例类的
        /// </summary>
        public static void DisposeAllSingleton()
        {
            for(int i= _allList.Count - 1; i>=0; i--)
            {
                var clearFunc = _allList[i];
                try
                {
                    clearFunc.Invoke();
                }
                catch (Exception e)
                {
                    Logger.LogError("DisposeAllSingleton Err:" + e);
                }
            }
        }
    }
}