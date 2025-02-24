using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonExtension : Button
{
    #region 单机双击相关
    public bool singleClickEnabled = true;
    public bool doubleClickEnabled = false;
    public float doubleClickTime = 0.3f;

    private float lastClickTime = float.NegativeInfinity;
    private int clickCount = 0;

    [FormerlySerializedAs("onDoubleClick")]
    [SerializeField]
    private ButtonClickedEvent doubleClickEvent = new ();

    public ButtonClickedEvent onDoubleClick
    {
        get => doubleClickEvent;
        set => doubleClickEvent = value;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if(!IsActive() && !interactable)
            return;

        if (singleClickEnabled)
        {
            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick?.Invoke();
            Debug.Log("单击");
        }

        if (doubleClickEnabled)
        {
            clickCount++;
            if (clickCount >= 2)
            {
                if (Time.realtimeSinceStartup - lastClickTime < doubleClickTime)
                {
                    UISystemProfilerApi.AddMarker("Button.onDoubleClick", this);
                    onDoubleClick?.Invoke();
                    Debug.Log("双击");
                    lastClickTime = float.NegativeInfinity;
                    clickCount = 0;
                }
                else
                {
                    clickCount = 1;
                    lastClickTime = Time.unscaledTime;
                }
            }
            else
            {
                lastClickTime = Time.unscaledTime;
            }
        }
    }
    #endregion

    #region 长按相关
    public bool longPressEnabled = false;

    private float lastPressTime = 0;

    public float minPressTime = 0.5f;

    private bool isPressing = false;

    private bool hasInvokedLongPress = false;

    [FormerlySerializedAs("onLongPress")]
    [SerializeField]
    private ButtonClickedEvent longPressEvent = new();

    public ButtonClickedEvent onLongPress
    {
        get => longPressEvent;
        set => longPressEvent = value;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (longPressEnabled)
        {
            hasInvokedLongPress = false;
            isPressing = true;
            lastPressTime = Time.unscaledTime;
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        isPressing = false;
        hasInvokedLongPress = false;
    }

    private void Update()
    {
        DealLongPress();
    }

    private void DealLongPress()
    {
        if(hasInvokedLongPress) return;
        if (isPressing)
        {
            if (Time.unscaledTime - lastPressTime >= minPressTime)
            {
                UISystemProfilerApi.AddMarker("Button.onLongPress", this);
                onLongPress?.Invoke();
                hasInvokedLongPress = true;
                Debug.Log("执行长按事件");
            }
        }
    }
    #endregion
}
