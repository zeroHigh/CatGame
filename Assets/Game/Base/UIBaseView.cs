using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game
{
    public abstract class UIBaseView : IDisposable
    {
        protected Transform transform;
        public RectTransform RectTransform { get; private set; }
        public GameObject DisplayObject { get; protected set; }
        private bool IsShow { get; set; }

        private List<Button> allButtons;

        #region Button事件处理函数
        protected void ListenButton(Button btn, UnityAction action)
        {
            if (btn == null)
                return;
            btn.onClick.AddListener(action);
            allButtons.Add(btn);
        }

        protected void UnListenButton(Button btn, UnityAction action)
        {
            if (btn == null)
                return;

            if (!allButtons.Contains(btn))
                return;

            btn.onClick.RemoveListener(action);
            allButtons.Remove(btn);
        }
        #endregion

        public void SetDisplayObject(GameObject gameViewRoot)
        {
            DisplayObject = gameViewRoot;
            if (DisplayObject == null)
            {
                Logger.LogError("[UIBaseView.SetDisplayObject() 显示对象为Null, ViewName:" + this.GetType().Name + "]");
                return;
            }

            transform = DisplayObject.transform;
            RectTransform = transform as RectTransform;
            allButtons = new List<Button>();

            var canvas = gameViewRoot.GetComponent<Canvas>();
            if (canvas != null)
                canvas.worldCamera = Camera.main;

            var dynamicChildRoot = gameViewRoot.transform as RectTransform;

            // if (!GlobalGameSetting.IsLandScape && GlobalGameSetting.IsNotchScreen)
            // {
            //     if (dynamicChildRoot != null) dynamicChildRoot.offsetMax = new Vector2(0f, -72f);
            // }
            // else
            //固定横屏，不用考虑刘海
            if (dynamicChildRoot != null) dynamicChildRoot.offsetMax = Vector2.zero;
            ParseComponent();
        }

        public virtual void Show(params object[] arg)
        {
            if (DisplayObject == null)
                return;
            DisplayObject.SetActive(true);
            if (!IsShow)
            {
                IsShow = true;
                AddEvent();
            }

            Refresh(arg);
        }

        protected virtual void Refresh(params object[] arg)
        {
        }

        public virtual void Hide()
        {
            if (DisplayObject != null && DisplayObject.activeSelf)
                DisplayObject.SetActive(false);
            if (!IsShow)
                return;
            IsShow = false;
            RemoveEvent();
        }

        protected GameObject Find(string path)
        {
            Transform childTF = transform.Find(path);
            return childTF == null ? null : childTF.gameObject;
        }

        protected T Find<T>(string path) where T : Component
        {
            return UnityObjectHelper.Find<T>(transform, path);
        }


        public virtual void Dispose()
        {
            RemoveEvent();
            if (allButtons != null)
            {
                for (int i = 0; i < allButtons.Count; i++)
                    allButtons[i].onClick.RemoveAllListeners();
                allButtons.Clear();
                allButtons = null;
            }

            UnityObjectHelper.DestroyGameObjectSafe(DisplayObject);
            DisplayObject = null;
            transform = null;
            RectTransform = null;
            IsShow = false;
        }

        public void SetParent(RectTransform parent, Vector2 pos = default)
        {
            if (parent == null || RectTransform == null)
            {
                Logger.Log(
                    $"UIBaseView-SetParent object is null. Please be careful! parent:{parent != null} RectTransform:{RectTransform != null}");
                return;
            }

            transform.SetParent(parent, false);
            RectTransform.anchoredPosition = pos;
        }

        protected virtual void AddEvent()
        {
        }

        protected virtual void RemoveEvent()
        {
        }

        protected abstract void ParseComponent();
    }
}