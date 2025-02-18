﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 功能：异步操作抽象基类，继承自IEnumerator接口，支持迭代，主要是为了让异步操作能够适用于协程操作
/// 注意：提供对协程操作的支持，但是异步操作的进行不依赖于协程，可以在Update等函数中查看进度值
/// </summary>

namespace Game
{
    public abstract class BaseAsyncOperation : IEnumerator, IDisposable
    {
        public object Current
        {
            get
            {
                return null;
            }
        }

        public bool isDone
        {
            get
            {
                return IsDone();
            }
        }

        public float progress
        {
            get
            {
                return Progress();
            }
        }

        abstract public void Update();

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
            // 解决sonar扫描问题
            Debug.Log("BaseAsyncOperation Reset");
        }

        abstract public bool IsDone();

        abstract public float Progress();

        public virtual void Dispose()
        {
        }
    }

    abstract public class BaseAssetBundleAsyncRequest : BaseAsyncOperation
    {
        public string assetbundleName
        {
            get;
            protected set;
        }

        //当前加载的资源名
        public string assetPath
        {
            get;
            protected set;
        }

        public AssetBundle assetbundle
        {
            get;
            protected set;
        }

        public override void Dispose()
        {
            assetbundleName = null;
            assetbundle = null;
        }
    }

    abstract public class BaseAssetAsyncLoader : BaseAsyncOperation
    {
        public UnityEngine.Object asset
        {
            get;
            protected set;
        }

        public override void Dispose()
        {
            asset = null;
        }
    }

    public class AssetBundleCreateRequester {

        public string abPath;
        public UnityWebRequest abCreaterRequest;
        public AssetBundleCreateRequester(string path, UnityWebRequest creater) {
            abPath = path;
            abCreaterRequest = creater;
            abCreaterRequest?.SendWebRequest();
        }


        public AssetBundle assetbundle
        {
            get
            {
                return DownloadHandlerAssetBundle.GetContent(abCreaterRequest);
            }
        }

        public void Dispose() {
            abPath = null;
            abCreaterRequest?.Dispose();
            abCreaterRequest = null;
        }
    }
}
