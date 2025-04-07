using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class WindowManager : Singleton<WindowManager>
    {
        private RectTransform bottomRoot;
        private RectTransform midRoot;
        private RectTransform topRoot;
        private RectTransform guideRoot;
        private Camera UICamera { get; set; }

        private GameObject uiRoot;
        private float realSizeW = 1440f;
        private float realSizeH = 800f;
        private float offsetW;
        private float offsetH;

        private float screenReallyW;
        private float screenReallyH;
        private float screenScale;

        public void Init(Transform root)
        {
            uiRoot = root.Find("UIRoot").gameObject;
            UICamera = root.Find("UICamera").GetComponent<Camera>();
            bottomRoot = root.Find("UIRoot/BottomRoot") as RectTransform;
            midRoot = root.Find("UIRoot/MiddleRoot") as RectTransform;
            topRoot = root.Find("UIRoot/TopRoot") as RectTransform;
            guideRoot = root.Find("UIRoot/GuideRoot") as RectTransform;
            Vector3 midRootLocalPos = midRoot.localPosition;
            midRootLocalPos.z = -60;
            midRoot.localPosition = midRootLocalPos;
            Vector3 topRootLocalPos = topRoot.localPosition;
            topRootLocalPos.z = -60;
            topRoot.localPosition = topRootLocalPos;
            Vector3 guideRootLocalPos = guideRoot.localPosition;
            guideRootLocalPos.z = -60;
            guideRoot.localPosition = guideRootLocalPos;

            UICamera.clearFlags = CameraClearFlags.SolidColor;
            UICamera.backgroundColor = new Color(0, 0, 0, 0);
        }

        public void AdjustScreenFit()
        {
            if(GlobalGameSetting.IsLandScape)
                InitScreenFit(1440, 800);
            else
                InitScreenFit(800, 1440);
        }

        private void InitScreenFit(float width, float height)
        {
            if (uiRoot == null)
            {
                Logger.Log("WindowManager 未调用Init初始化方法");
            }
            var scaler = uiRoot.GetComponent<CanvasScaler>();

            if (GlobalGameSetting.IsLandScape)
            {
                screenReallyW = Mathf.Max(Screen.width, Screen.height);
                screenReallyH = Mathf.Min(Screen.width, Screen.height);
            }
            else
            {
                screenReallyW = Mathf.Min(Screen.width, Screen.height);
                screenReallyH = Mathf.Max(Screen.width, Screen.height);
            }
            var scaleW = screenReallyW / width;
            var scaleH = screenReallyH / height;
            screenScale = Math.Min(scaleW, scaleH);
            realSizeW = screenReallyW / screenScale;
            realSizeH = screenReallyH / screenScale;
            scaler.referenceResolution = new Vector2(realSizeW, realSizeH);
            offsetW = (realSizeW - width) / 2;
            offsetH = (realSizeH - height) / 2;
            Logger.Log($"当前屏幕尺寸：【w : {screenReallyW}, h : {screenReallyH}】, 缩放值：【scale : {screenScale}】");
            Logger.Log($"适配后屏幕尺寸：【w : {realSizeW}, h : {realSizeH}】");
            Logger.Log($"偏移量：【w : {offsetW}, h : {offsetH}】");

        }

        public override void Dispose()
        {
            ShowOrHideUIRoot(true);
            dictAllWindows.Clear();
            dictShowWindows.Clear();
            dictHideStack.Clear();
            _instance = null;
        }

        private readonly Dictionary<uint, UIWindowBase> dictAllWindows = new Dictionary<uint, UIWindowBase>();
        private readonly Dictionary<WindowLayer, UIWindowBase> dictShowWindows = new Dictionary<WindowLayer, UIWindowBase>();
        private readonly Dictionary<WindowLayer, Stack<uint>> dictHideStack = new Dictionary<WindowLayer, Stack<uint>>();


        private void ShowOrHideUIRoot(bool isShow)
        {
            uiRoot?.SetActive(isShow);
        }

        public RectTransform GetUIRootByLayer(WindowLayer layer)
        {
            switch (layer)
            {
                case WindowLayer.Bottom:
                    return bottomRoot;
                case WindowLayer.Middle:
                    return midRoot;
                case WindowLayer.Top:
                    return topRoot;
                case WindowLayer.Guide:
                    return guideRoot;
            }

            return null;
        }
    }
}