using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
   
    private Stack<BasePanel> panelStack = new Stack<BasePanel>();  // record current windows
    
    private void Awake() => Instance = this;

    public void OpenPanel(BasePanel panel)
    {
        // has other panel, pause it (pause the last)
        if (panelStack.Count > 0)
        {
            panelStack.Peek().OnPause();
        }


        panelStack.Push(panel); // open new panel
        panel.OnEnter();
    }

    public void CloseTopPanel()
    {
        if (panelStack.Count == 0) return;

        BasePanel topPanel = panelStack.Pop();
        topPanel.OnExit();

        // resume the last panel
        if (panelStack.Count > 0)
        {
            panelStack.Peek().OnResume();
        }
    }
}
