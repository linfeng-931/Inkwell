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

        //Debug.Log($"��ҤF: {panel.gameObject.name}, Stack �ѤU {panelStack.Count} ��");

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

        if (panelStack.Count > 0)
        {
            BasePanel nextPanel = panelStack.Peek();
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
