using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private Stack<BasePanel> panelStack = new Stack<BasePanel>();  // record current windows
    
    private void Awake() => Instance = this;

    private void OnEnable()
    {
        GameEvent.OnEscapePressed += CloseTopPanel;
    }

    private void OnDisable()
    {
        GameEvent.OnEscapePressed -= CloseTopPanel;
    }

    public void OpenPanel(BasePanel panel)
    {
        if (panelStack.Count > 0 && panelStack.Peek() == panel)
        {
            panel.OnEnter();
            return;
        }

        Debug.Log($"開啟了: {panel.gameObject.name}, Stack 剩下 {panelStack.Count} 個");

        foreach(BasePanel basePanel in panelStack)
        {
            Debug.Log(basePanel.gameObject.name);
        }

        if (panelStack.Count > 0)
        {
            panelStack.Peek().OnPause();
        }

        panelStack.Push(panel);
        panel.OnEnter();
    }

    public void CloseTopPanel()
    {
        if (panelStack.Count == 0) return;

        BasePanel topPanel = panelStack.Pop();
        topPanel.OnExit();

        Debug.Log($"關閉了: {topPanel.gameObject.name}, Stack 剩下 {panelStack.Count} 個");

        if (panelStack.Count > 0)
        {
            BasePanel nextPanel = panelStack.Peek();
            Debug.Log($"準備喚醒: {nextPanel.gameObject.name}");

            if (topPanel == nextPanel)
            {
                Debug.LogError("警告：Stack 中存在兩個一模一樣的面板參考！");
            }

            nextPanel.OnResume();
        }
        else if(panelStack.Count == 0)
        {
            GameEvent.OnInteractStateChanged.Invoke(false);

            GameEvent.currentCloseCallback?.Invoke();
            GameEvent.currentCloseCallback = null;
        }
    }
}
