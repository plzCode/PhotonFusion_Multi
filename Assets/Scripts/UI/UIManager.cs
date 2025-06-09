using FusionExamples.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    readonly Dictionary<ECanvasType, Canvas> _canvasCache = new();
    readonly Dictionary<Type, Base_UI> _uiCache = new();
    readonly List<Base_UI> _uiStack = new();

    public enum ECanvasType
    {
        Default,
        Combat,
        Other,
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_uiStack.Count > 0)
            {
                Base_UI topUI = _uiStack[_uiStack.Count - 1];
                CloseUI(topUI);
            }
        }
    }

    public Canvas GetCanvas(ECanvasType type)
    {
        Canvas canvas;

        if (!_canvasCache.TryGetValue(type, out canvas))
        {
            Canvas_UI[] canvases = FindObjectsByType<Canvas_UI>(FindObjectsSortMode.None);

            foreach (var canvas_ui in canvases)
                _canvasCache.Add(canvas_ui.Type, canvas_ui.GetComponent<Canvas>());

            if (!_canvasCache.TryGetValue(type, out canvas))
            {
                Canvas canvasPrefab = Resources.Load<Canvas>($"UI/Canvas/{type} Canvas");
                canvas = Instantiate(canvasPrefab, transform);
            }
        }

        return canvas;
    }

    public T GetUI<T>() where T : Base_UI
    {
        if (_uiCache.TryGetValue(typeof(T), out var ui))
        {
            return (T)ui;
        }

        // Instantiate UI
        T prefab = Resources.Load<T>($"UI/{typeof(T).Name}");
        
        if (prefab == null)
        {
            Debug.LogError($"UI �������� ã�� �� �����ϴ�. {typeof(T).Name}");
            return null;
        }

        GameObject spawnedObj = Instantiate(prefab.gameObject);
        spawnedObj.name = typeof(T).Name;
        _uiCache[typeof(T)] = spawnedObj.GetComponent<T>();
        return (T)_uiCache[typeof(T)];
    }

    public void ShowUI<T>(Base_UIInfo info) where T : Base_UI
    {
        T ui = GetUI<T>();
        ShowUI(ui, info);
    }

    public void ShowUI(Base_UI ui, Base_UIInfo info)
    {
        if (ui.gameObject == null)
        {
            Debug.LogError($"UI�� �������� �ʽ��ϴ�.");
            return;
        }

        if (ui.gameObject.activeSelf)
        {
            Debug.LogWarning($"UI�� �̹� Ȱ��ȭ�Ǿ� �ֽ��ϴ�.");
            return;
        }

        ui.Initialize();
        ui.SetInfo(info);
        ui.gameObject.SetActive(true);
        ui.OnOpen();

        if (ui.IsStackable)
            _uiStack.Add(ui);
    }

    public void CloseUI<T>() where T : Base_UI
    {
        T ui = GetUI<T>();
        CloseUI(ui);
    }

    public void CloseUI(Base_UI ui)
    {
        if (ui == null)
        {
            Debug.LogError($"UI�� �������� �ʽ��ϴ�.");
            return;
        }

        if (!ui.gameObject.activeSelf)
        {
            Debug.LogWarning($"UI�� �̹� ��Ȱ��ȭ�Ǿ� �ֽ��ϴ�.");
            return;
        }

        ui.OnClose();
        ui.gameObject.SetActive(false);

        if (ui.IsStackable)
        {
            _uiStack.Remove(ui);
        }
    }
}
