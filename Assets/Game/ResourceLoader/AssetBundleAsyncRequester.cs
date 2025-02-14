using System;
using System.Collections.Generic;

/// <summary>
/// 功能：Assetbundle加载器，给逻辑层使用（预加载），支持协程操作
/// 注意：
/// 1、加载器AssetBundleManager负责调度，创建，回收，暂时不暴露给逻辑层
/// 2、可以选择是否缓存
/// </summary>
namespace Game
{
    public class AssetBundleAsyncRequester : BaseAssetBundleAsyncRequest
    {
        static Queue<AssetBundleAsyncRequester> pool = new Queue<AssetBundleAsyncRequester>();
        static int sequence = 0;
        public List<string> waitingList = new List<string>();
        protected int waitingCount = 0;
        protected bool isOver = false;

        public Action<string,bool> callbackAction;

        public static AssetBundleAsyncRequester Get(bool IsCache = false)
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                return new AssetBundleAsyncRequester(++sequence);
            }
        }

        public static void Recycle(AssetBundleAsyncRequester request)
        {
            pool.Enqueue(request);
        }

        public AssetBundleAsyncRequester(int sequence,bool IsCache = false)
        {
            Sequence = sequence;
        }

        public void Init(string name, string[] dependances, System.Action<string, bool> callback = null)
        {
            assetbundleName = name;
            isOver = false;
            callbackAction = callback;
            waitingList.Clear();
            // 说明：只添加没有被加载过的
            assetbundle = AssetBundleUtil.GetAssetBundleCache(assetbundleName);
            if (assetbundle == null)
            {
                waitingList.Add(assetbundleName);
            }

            if (dependances != null && dependances.Length > 0)
            {
                for (int i = 0; i < dependances.Length; i++)
                {
                    var ab = dependances[i];
                    if (!AssetBundleUtil.IsAssetBundleLoaded(ab))
                    {
                        waitingList.Add(dependances[i]);
                    }
                }
            }
            waitingCount = waitingList.Count;
        }

        public int Sequence
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

            for (int i = waitingList.Count - 1; i >= 0; i--)
            {
                if (AssetBundleUtil.IsAssetBundleLoaded(waitingList[i]))
                {
                    var curFinished = waitingList[i];
                    if (curFinished == assetbundleName)
                    {
                        assetbundle = AssetBundleUtil.GetAssetBundleCache(assetbundleName);
                    }
                    waitingList.RemoveAt(i);
                }
            }
            //即使等待队列一开始就是0，也必须让ResourceLoader跑一次update，它要善后
            isOver = waitingList.Count == 0;
        }

        public override void Dispose()
        {
            waitingList.Clear();
            waitingCount = 0;
            assetbundleName = null;
            assetbundle = null;
            callbackAction = null;
            Recycle(this);
        }

        public override float Progress()
        {
            if (isDone)
            {
                return 1.0f;
            }

            float progressSlice = 1.0f / waitingCount;
            float progressValue = (waitingCount - waitingList.Count) * progressSlice;
            for (int i = waitingList.Count - 1; i >= 0; i--)
            {
                var cur = waitingList[i];
                var creater =  AssetBundleUtil.GetAssetBundleAsyncCreater(cur);
                progressValue += (creater != null ? creater.downloadProgress : 1.0f) * progressSlice;
            }
            return progressSlice;
        }
    }
}
