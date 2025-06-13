using System;
using UnityEngine;

public class Base_UIInfo
{
    public Action OnOpenEvent;
    public Action OnCloseEvent;
}

public abstract class Base_UI : MonoBehaviour
{
    public UIManager.ECanvasType CanvasType;
    public bool IsStackable;
    public bool IsOpened { get; private set; }

    public event Action OnOpenEvent;
    public event Action OnCloseEvent;
    
    Animation _openAnimation;

    public virtual void Initialize()
    {
        OnOpenEvent = null;
        OnCloseEvent = null;

        Canvas canvas = UIManager.Instance.GetCanvas(CanvasType);
        transform.SetParent(canvas.transform);

        var rectTransform = GetComponent<RectTransform>();
        _openAnimation = GetComponent<Animation>();

        rectTransform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.localScale = new Vector3(1f, 1f, 1f);
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);
    }

    public virtual void SetInfo(Base_UIInfo info)
    {
        OnOpenEvent = info.OnOpenEvent;
        OnCloseEvent = info.OnCloseEvent;
    }

    public virtual void OnOpen()
    {
        if (_openAnimation)
            _openAnimation.Play();

        OnOpenEvent?.Invoke();
        IsOpened = true;
    }

    public virtual void OnClose()
    {
        OnCloseEvent?.Invoke();
        IsOpened = false;
    }
}
