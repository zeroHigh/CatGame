using System.Collections.Generic;

/// <summary>
/// 功能：Asset异步加载器，自动追踪依赖的ab加载进度
/// 说明：一定要所有ab都加载完毕以后再加载asset，所以这里分成两个加载步骤
/// </summary>

namespace Game
{

    public class AssetAsyncRequester : BaseAssetAsyncLoader
    {
        static Queue<AssetAsyncRequester> pool = new Queue<AssetAsyncRequester>();
        static int sequence = 0;
        protected bool isOver = false;
        protected string[] abDependencies = null;//依赖的ab包

        public System.Action<UnityEngine.Object> callbackAction;

        public static AssetAsyncRequester Get()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                return new AssetAsyncRequester(++sequence);
            }
            
        }

        public static void Recycle(AssetAsyncRequester creater)
        {
            pool.Enqueue(creater);
        }

        public AssetAsyncRequester(int sequence)
        {
            Sequence = sequence;
        }

        public void Init(string assetName, UnityEngine.Object asset, string abName, System.Action<UnityEngine.Object> action)
        {
            AssetName = assetName;
            this.asset = asset;
            abDependencies = null;
            isOver = true;
            callbackAction = action;
        }

        public int Sequence
        {
            get;
            protected set;
        }

        public void Init(string assetName, string[] dependencies, string abName, System.Action<UnityEngine.Object> action)
        {
            AssetName = assetName;
            this.asset = null;
            isOver = false;
            abDependencies = dependencies;
            callbackAction = action;

            for (int i = 0; i < abDependencies.Length; i++) {
                abDependencies[i] = AssetBundleUtil.GetAssetBundleFullPath(abDependencies[i]);
            }
        }

        public string AssetName
        {
            get;
            protected set;
        }

        public override bool IsDone()
        {
            return isOver; 
        }

        public override void Update()
        {
            if (isDone)
            {
                return;
            }

            isOver = IsLoaingOver();
            if (!isOver)
            {
                return;
            }
            asset = AssetBundleUtil.LoadResByFile<UnityEngine.Object>(AssetName);
        }

        private bool IsLoaingOver() {
            if (abDependencies == null)
                return false;
            foreach (string abPath in abDependencies) {
                var ab = AssetBundleUtil.GetAssetBundleCache(abPath);
                if (ab == null)
                    return false;
            }
            return true;
        }

        public override void Dispose()
        {
            isOver = true;
            AssetName = null;
            asset = null;
            callbackAction = null;
            Recycle(this);
        }

        public override float Progress()
        {
            return 1f;
        }
    }
}
