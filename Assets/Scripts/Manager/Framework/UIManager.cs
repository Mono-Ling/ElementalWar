using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingleMono<UIManager>
{
    public const string FOLDER_PATH = "UI";
    public const string CANVAS_PATH = "Canvas";
    public const string EVENT_SYSTEM_PATH = "EventSystem";
    private Transform _dynamicCanvas;
    private Transform _staticCanvas;
    private Stack<BaseUI> _panelStack = new();
    private BaseUI CurrentPanel
    {
        get
        {
            if (_panelStack.Count == 0)
                return null;
            return _panelStack.Peek();
        }
    }
    void Awake()
    {
        var canvasPrefab = Resources.Load<GameObject>($"{FOLDER_PATH}/{CANVAS_PATH}");
        if (canvasPrefab == null)
        {
            Debug.LogError("【UI管理器】Canvas加载失败");
            return;
        }
        var canvasObj = Instantiate(canvasPrefab, transform, false);
        if (canvasObj == null)
        {
            Debug.LogError("【UI管理器】Canvas创建失败");
            return;
        }
        _dynamicCanvas = canvasObj.transform;
        canvasObj = Instantiate(canvasPrefab, transform, false);
        _staticCanvas = canvasObj.transform;

        var eventPrefab = Resources.Load<GameObject>($"{FOLDER_PATH}/{EVENT_SYSTEM_PATH}");
        if (eventPrefab == null)
        {
            Debug.LogError("【UI管理器】EventSystem加载失败");
            return;
        }
        var eventObj = Instantiate(eventPrefab, transform, false);
        if (eventObj == null)
            Debug.LogError("【UI管理器】EventSystem创建失败");
    }
    public bool TryGetCurrentPanel<T>(out T panel) where T : BaseUI
    {
        if (CurrentPanel is T findPanel)
        {
            panel = findPanel;
            return true;
        }
        Debug.LogWarning($"【UI管理器】面板{typeof(T)}不存在");
        panel = null;
        return false;
    }
    public T ShowPanel<T>(Action<BaseUI> action = null, bool isAnimation = true) where T : BaseUI
    {
        var type = typeof(T);
        if (CurrentPanel is T findePanel)
        {
            Debug.LogWarning($"【UI管理器】{type}已存在");
            return findePanel;
        }
        var path = type.Name;
        var panelPrefab = Resources.Load<GameObject>($"{FOLDER_PATH}/{path}");
        if (panelPrefab == null)
        {
            Debug.LogError($"【UI管理器】面板{path}加载失败");
            return null;
        }

        var panelObj = Instantiate(panelPrefab, _staticCanvas, false);
        if (panelObj == null)
        {
            Debug.LogError($"【UI管理器】面板{typeof(T)}创建失败");
            return null;
        }

        if (!InitPanel<T>(panelObj, out var panel))
        {
            Debug.LogError($"【UI管理器】面板{path}初始化失败");
            return null;
        }

        CurrentPanel?.Hide(DisablePanel, isAnimation);

        panel.Show(action, isAnimation);
        _panelStack.Push(panel);
        return panel;
    }
    public IEnumerator ShowPanelAsync<T>(Action<T> asyncCallback = null, Action<BaseUI> showCallback = null, bool isAnimation = true) where T : BaseUI
    {
        var type = typeof(T);
        if (CurrentPanel is T findePanel)
        {
            Debug.LogWarning($"【UI管理器】{type}已存在");
            yield break;
        }
        var path = type.Name;
        var req = Resources.LoadAsync<GameObject>($"{FOLDER_PATH}/{path}");
        yield return req;

        if (req.asset is not GameObject panelPrefab)
        {
            Debug.LogError($"【UI管理器】面板{path}加载失败");
            asyncCallback?.Invoke(null);
            yield break;
        }

        var instantiateReq = InstantiateAsync(panelPrefab);
        yield return instantiateReq;

        if (instantiateReq.Result[0] is not GameObject panelObj)
        {
            Debug.LogError($"【UI管理器】面板{typeof(T)}创建失败");
            asyncCallback?.Invoke(null);
            yield break;
        }
        if (!InitPanel<T>(panelObj, out var panel))
        {
            Debug.LogError($"【UI管理器】面板{path}初始化失败");
            asyncCallback?.Invoke(null);
            yield break;
        }
        CurrentPanel?.Hide(DisablePanel, isAnimation);

        panelObj.transform.SetParent(_staticCanvas, false);
        panel.Show(showCallback, isAnimation);
        _panelStack.Push(panel);
        asyncCallback?.Invoke(panel);
    }
    public void HidePanel(Action<BaseUI> action = null, bool isAnimation = true)
    {
        if (CurrentPanel == null)
        {
            Debug.LogWarning("【UI管理器】当前没有打开面板");
            return;
        }
        action += DestroyPanel;
        CurrentPanel.Hide(action, isAnimation);
        _panelStack.Pop();
        EnablePanel(CurrentPanel);
        CurrentPanel?.Show(null, isAnimation);
    }
    public T BufferShowUI<T>(Action<BaseUI> action = null, bool isAnimation = true) where T : BaseUI
    {
        var path = typeof(T).Name;
        var obj = MonoObjectPool.Instance.GetObject($"{FOLDER_PATH}/{path}");
        if (obj == null)
        {
            Debug.LogError($"【UI管理器】UI{path}创建失败");
            return null;
        }
        var ui = obj.GetComponent<T>();
        if (ui == null)
        {
            Debug.LogError($"【UI管理器】UI{path}获取失败");
            MonoObjectPool.Instance.PutObject(obj);
            return null;
        }
        obj.transform.SetParent(_dynamicCanvas, false);
        ui.Show(action, isAnimation);
        return ui;
    }
    public void BufferHideUI(BaseUI uI, Action<BaseUI> action = null, bool isAnimation = true)
    {
        if (uI == null)
            return;
        action += BufferDestroyUI;
        uI.Hide(action, isAnimation);
    }
    /// <summary>
    /// 屏幕坐标 → 动态画布平面世界坐标
    /// </summary>
    public bool ScreenPointToDynamicCanvasWorld(Vector2 screenPoint, out Vector3 worldPoint)
    {
        worldPoint = default;
        var rect = _dynamicCanvas as RectTransform;
        if (rect == null)
            return false;
        var canvas = rect.GetComponentInParent<Canvas>();
        if (canvas == null)
            return false;
        return RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, canvas.rootCanvas.worldCamera, out worldPoint);
    }
    public static void InitUIPosition(BaseUI uI)
    {
        if (uI == null)
            return;
        if (uI.transform is not RectTransform rect)
            return;
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
    }
    private bool InitPanel<T>(GameObject panelObj, out T panel) where T : BaseUI
    {
        panel = null;
        if (panelObj == null)
            return false;
        panel = panelObj.GetComponent<T>();
        if (panel == null)
        {
            Debug.LogError($"【UI管理器】面板{typeof(T)}获取失败失败");
            Destroy(panelObj);
            return false;
        }
        return true;
    }
    private void DisablePanel(BaseUI uI)
    => uI?.gameObject?.SetActive(false);
    private void EnablePanel(BaseUI uI)
    => uI?.gameObject?.SetActive(true);
    private void DestroyPanel(BaseUI uI)
    => Destroy(uI.gameObject);
    private void BufferDestroyUI(BaseUI uI)
    => MonoObjectPool.Instance.PutObject(uI.gameObject);
}