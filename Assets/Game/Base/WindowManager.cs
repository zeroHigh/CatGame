using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public enum FitMode
    {
        FixedHeight,    //顶高适配
        FixedWidth,     //顶宽适配
        FixedCenter,    //保证安全区域的适配
        NoBorder        //以覆盖全屏幕的方式展示画布内容，设计尺寸和屏幕分辨率比例不等的情况下，一个方向上顶满，另一个方向会超出屏幕
    }

    public class WindowManager : ILSingleton<WindowManager>
    {
        private RectTransform bottomRoot;
        private RectTransform midRoot;
        private RectTransform topRoot;
        private RectTransform guideRoot;
        private Canvas windowCanvas;
        public Camera UICamera { get; private set; }

        private GameObject uiRoot;
        private float realSizeW = 1440f;
        private float realSizeH = 900f;
        private float offsetW;
        private float offsetH;

        private float screenReallyW;
        private float screenReallyH;
        private float screenScale;

        public float RealSizeW
        {
            get { return realSizeW; }
        }

        public float RealSizeH
        {
            get { return realSizeH; }
        }

        public float ScreenScale
        {
            get { return screenScale; }
        }

        public float OffsetW
        {
            get { return offsetW; }
        }
        public float OffsetH
        {
            get { return offsetH; }
        }
        public void Init(Transform root)
        {
            windowCanvas = root.Find("UIRoot").GetComponent<Canvas>();
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
                InitScreenFit(1440, 900);
            else
                InitScreenFit(900, 1440);
        }

        public void InitScreenFit(float width, float height)
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

        public void SetScreenFit(float width, float height, FitMode mode)
        {
            if (uiRoot == null)
            {
                Logger.Log("WindowManager 未调用Init初始化方法");
            }
            var scaler = uiRoot.GetComponent<CanvasScaler>();
            float reallyW, reallyH;
            if (GlobalGameSetting.IsLandScape)
            {
                reallyW = Mathf.Max(Screen.width, Screen.height);
                reallyH = Mathf.Min(Screen.width, Screen.height);
            }
            else
            {
                reallyW = Mathf.Min(Screen.width, Screen.height);
                reallyH = Mathf.Max(Screen.width, Screen.height);
            }

            var scaleW = reallyW / width;
            var scaleH = reallyH / height;
            var scaleMin = Math.Min(scaleW, scaleH);
            var scaleMax = Math.Max(scaleW, scaleH);
            switch (mode)
            {
                case FitMode.FixedCenter:
                    realSizeW = reallyW / scaleMin;
                    realSizeH = reallyH / scaleMin;
                    break;
                case FitMode.FixedHeight:
                    realSizeW = reallyW / scaleH;
                    realSizeH = reallyH / scaleH;
                    break;
                case FitMode.FixedWidth:
                    realSizeW = reallyW / scaleW;
                    realSizeH = reallyH / scaleW;
                    break;
                case FitMode.NoBorder:
                    realSizeW = reallyW / scaleMax;
                    realSizeH = reallyH / scaleMax;
                    break;
                default:
                    realSizeW = reallyW / scaleMin;
                    realSizeH = reallyH / scaleMin;
                    break;
            }
            scaler.referenceResolution = new Vector2(realSizeW, realSizeH);
            offsetW = (realSizeW - width) / 2;
            offsetH = (realSizeH - height) / 2;
            Logger.Log($"当前屏幕尺寸：【w : {reallyW}, h : {reallyH}】");
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
        public void RegisterWindow(uint windowId, UIWindowBase uiWindow)
        {
            if (dictAllWindows.ContainsKey(windowId))
            {
                Logger.LogRed("[WindowManager.AddWindow() => 重复添加Window,ID:" + windowId + "]");
                return;
            }
            dictAllWindows.Add(windowId, uiWindow);
        }

        public void OpenWindow(uint windowId, params object[] args)
        {
            if (!dictAllWindows.ContainsKey(windowId))
            {
                Logger.Log("[WindowManager.OpenWindow() => windowId:" + windowId + "未注册，打开失败]");
                return;
            }

            UIWindowBase window = dictAllWindows[windowId];
            if (dictShowWindows.ContainsKey(window.Layer))
            {
                UIWindowBase curWindow = dictShowWindows[window.Layer];
                if (curWindow.WindowId == windowId)
                {
                    curWindow.Show(args);
                    return;
                }
                if (curWindow.Stack())
                {
                    Stack<uint> hideStack;
                    if (dictHideStack.ContainsKey(curWindow.Layer))
                    {
                        hideStack = dictHideStack[curWindow.Layer];
                    }
                    else
                    {
                        hideStack = new Stack<uint>();
                        dictHideStack.Add(curWindow.Layer, hideStack);
                    }

                    if (hideStack.Contains(curWindow.WindowId))
                        Logger.LogRed("[WindowManager.OpenWindow() => 重复添加Window到隐藏堆栈,windowID:" + curWindow.WindowId + "]");
                    hideStack.Push(curWindow.WindowId);
                }
                curWindow.Hide();
            }
            dictShowWindows[window.Layer] = window;
            window.Show(args);
            AddWindowToStage(window);
        }

        public void CloseWindow(uint windowId)
        {
            if (!dictAllWindows.ContainsKey(windowId))
            {
                Logger.LogRed("[WindowManager.CloseWindow() => windowId:" + windowId + "未注册，关闭失败]");
                return;
            }
            UIWindowBase window = dictAllWindows[windowId];

            if (!dictShowWindows.ContainsKey(window.Layer) || dictShowWindows[window.Layer] != window)
            {
                Logger.LogRed("[WindowManager.CloseWindow() => 关闭window失败，当前window未打开,windowID:" + windowId + "]");
                return;
            }
            window.Hide();
            if (window.Stack() && dictHideStack.TryGetValue(window.Layer, out var stack) && stack.Count > 0)
            {
                OpenWindow(stack.Pop());
            }
        }

        public void CloseAllWindow()
        {
            for (var i = 0; i < dictShowWindows.Count; i++)
            {
                var kv = dictShowWindows.ElementAt(i);
                kv.Value?.Dispose();
            }
            dictShowWindows.Clear();

            for (var i = 0; i < dictHideStack.Count; i++)
            {
                var kv = dictHideStack.ElementAt(i);
                CloseMultiWindows(kv.Value);
            }

            ShowOrHideUIRoot(false);
        }

        public void ClearAllUIGameObject()
        {
            ClearChildGameObject(bottomRoot);
            ClearChildGameObject(midRoot);
            ClearChildGameObject(topRoot);
            ClearChildGameObject(guideRoot);
        }

        private void ClearChildGameObject(Transform parent)
        {
            if (parent == null)
            {
                return;
            }
            var count = parent.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private void CloseMultiWindows(Stack<uint> windows)
        {
            while (windows.Count > 0)
            {
                uint windowId = windows.Pop();
                if (dictAllWindows.ContainsKey(windowId))
                {
                    dictAllWindows[windowId].Dispose();
                }
            }
        }

        public void ShowOrHideUIRoot(bool isShow)
        {
            uiRoot?.SetActive(isShow);
        }

        private void AddWindowToStage(UIWindowBase windowBase)
        {
            AddUIViewToStage(windowBase, windowBase.Layer);
        }

        public void AddUIViewToStage(UIBaseView uiView, WindowLayer layer, int layerIndex = 0)//bool isFirst = true)
        {
            switch (layer)
            {
                case WindowLayer.Bottom:
                    uiView.SetParent(bottomRoot);
                    break;
                case WindowLayer.Middle:
                    uiView.SetParent(midRoot);
                    break;
                case WindowLayer.Top:
                    uiView.SetParent(topRoot);
                    //if (isFirst)
                    {
                        uiView.DisplayObject.transform.SetAsFirstSibling();
                        uiView.DisplayObject.transform.SetSiblingIndex(layerIndex);
                    }
                    break;
                case WindowLayer.Guide:
                    uiView.SetParent(guideRoot);
                    break;
            }
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

        public Vector3 WorldToUIPoint(Vector3 position)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(windowCanvas.transform as RectTransform, Camera.main.WorldToScreenPoint(position), windowCanvas.worldCamera, out pos);
            return pos;
        }

        public Vector2 ScreenPosToLocalPos(Vector2 screenPos)
        {
            return ScreenPosToRectLocalPos(windowCanvas.transform as RectTransform, screenPos);
        }

        public Vector2 ScreenPosToRectLocalPos(RectTransform rectTransform, Vector2 screenPos)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, UICamera, out pos);
            return pos;
        }

        /// <summary>
        /// 转换屏幕坐标为适配坐标
        /// </summary>
        /// <param name="screenPos">屏幕坐标</param>
        /// <returns></returns>
        public Vector2 ConvertFitPoint(Vector2 screenPos)
        {
            Vector2 ret = new Vector2();
            RectTransform rootTransform = windowCanvas.transform as RectTransform;
            if (rootTransform && RectTransformUtility.ScreenPointToLocalPointInRectangle(rootTransform, screenPos, UICamera, out ret))
            {
                float offsetX = rootTransform.rect.width * (rootTransform.pivot.x - 0f);
                float offsetY = rootTransform.rect.height * (rootTransform.pivot.y - 0f);
                ret.x = ret.x + offsetX;
                ret.y = ret.y + offsetY;
                return ret;
            }
            return ret;
        }

        /// <summary>
        /// 转换一个节点坐标到另外一个节点坐标
        /// </summary>
        /// <param name="srcNode">源节点</param>
        /// <param name="dstNode">目标节点</param>
        /// <param name="nodePos">源节点位置</param>
        /// <returns>目标节点位置</returns>
        public Vector2 ConvertNodeSpaceToNodeSpace(GameObject srcNode, GameObject dstNode, Vector2 nodePos)
        {
            Vector2 wordPoint = ConvertToWorldSpace(srcNode, nodePos);
            return ConvertToNodeSpace(dstNode, wordPoint);
        }

        /// <summary>
        /// 转换一个全屏坐标为节点下坐标
        /// </summary>
        /// <param name="node">目标节点</param>
        /// <param name="worldPos">全屏坐标</param>
        /// <returns>节点下坐标</returns>
        public Vector2 ConvertToNodeSpace(GameObject node, Vector2 worldPos)
        {
            Vector2 leftCorner = ConvertToWorldSpace(node, Vector2.zero);
            return new Vector2(worldPos.x - leftCorner.x, worldPos.y - leftCorner.y);
        }

        /// <summary>
        /// 转换一个节点坐标为全屏坐标（全屏是指根canvas）
        /// </summary>
        /// <param name="node">目标节点</param>
        /// <param name="nodePos">节点坐标</param>
        /// <returns>全屏坐标</returns>
        public Vector2 ConvertToWorldSpace(GameObject node, Vector2 nodePos)
        {
            if (uiRoot == null)
            {
                Logger.LogBlue("没有设置根节点");
                return Vector2.zero;
            }
            Vector2 ret = nodePos;
            RectTransform rt = node.transform as RectTransform;
            RectTransform rootTransform = windowCanvas.transform as RectTransform;
            while (rt != null && rt != rootTransform)
            {
                Vector2 anchorMin = rt.anchorMin;
                Vector2 minOffset = rt.offsetMin;
                rt = rt.parent as RectTransform;
                if (rt == null)
                {
                    break;
                }
                Rect parentRect = rt.rect;
                ret.x = ret.x + minOffset.x + anchorMin.x * parentRect.width;
                ret.y = ret.y + minOffset.y + anchorMin.y * parentRect.height;
            }
            return ret;
        }

        /// <summary>
        /// 传入安全区域坐标，获取适配后坐标，相对根节点左下角
        /// </summary>
        /// <param name="x">安全区横坐标</param>
        /// <param name="y">安全区纵坐标</param>
        /// <returns></returns>
        public Vector2 GetFitPoint(float x, float y)
        {
            return new Vector2(x + offsetW,y + offsetH);
        }

        /// <summary>
        /// 根据传入节点的anchor point转换坐标
        /// </summary>
        /// <param name="node">传入节点</param>
        /// <param name="nodePos">相对于传入节点的父节点左下角的一个坐标</param>
        /// <returns>返回相对于传入节点anchor point的一个坐标</returns>
        public Vector2 ConvertLbPointByAnchor(GameObject node, Vector2 nodePos)
        {
            RectTransform rt = node.transform as RectTransform;
            RectTransform parent = node.transform.parent as RectTransform;
            if (rt == null || parent == null)
            {
                return nodePos;
            }

            Vector2 anchor = rt.anchorMin;
            if (!rt.anchorMax.Equals(rt.anchorMin))
            {
                anchor = Vector2.zero;
            }
            Rect parentRect = parent.rect;
            Vector2 ret = new Vector2();
            ret.x = nodePos.x - parentRect.width * anchor.x;
            ret.y = nodePos.y - parentRect.height * anchor.y;

            return ret;
        }


        /// <summary>
        /// 世界坐标转UIRoot的本地坐标
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public Vector2 CovertWorldPosToUIRootLocalPos(Vector2 worldPos)
        {
            Vector2 screenPos = UICamera.WorldToScreenPoint(worldPos);
            Vector2 uIRootLocalPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(uiRoot.GetComponent<RectTransform>(), screenPos,
                UICamera, out uIRootLocalPos);
            return uIRootLocalPos;
        }

        /// <summary>
        /// 世界坐标转指定root的本地坐标
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public Vector2 CovertWorldPosToRootLocalPos(Vector2 worldPos, RectTransform root)
        {
            Vector2 screenPos = UICamera.WorldToScreenPoint(worldPos);
            Vector2 uIRootLocalPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root.GetComponent<RectTransform>(), screenPos,
                UICamera, out uIRootLocalPos);
            return uIRootLocalPos;
        }

        /// <summary>
        /// 屏幕坐标转指定root的本地坐标
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public Vector2 CovertScreenPosToRootLocalPos(Vector2 screenPos, RectTransform root)
        {
            Vector2 uIRootLocalPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root.GetComponent<RectTransform>(), screenPos,
                UICamera, out uIRootLocalPos);
            return uIRootLocalPos;
        }

        /// <summary>
        /// UI 坐标 转屏幕 坐标
        /// </summary>
        /// <param name="uiRootPos"> ui 坐标必须是根节点基于左下角为0,0 的坐标</param>
        /// <returns></returns>
        public Vector2 CovertUIRootToScreenPoint(Vector2 uiRootPos)
        {
            return uiRootPos * screenScale;
        }
    }
}