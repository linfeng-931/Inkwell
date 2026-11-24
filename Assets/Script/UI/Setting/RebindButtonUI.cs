using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindButtonUI : MonoBehaviour
{
    [Header("Binding Target")]
    public InputActionReference actionReference;
    [Tooltip("Action Binding（keyboard = 0, controller = 1）")]
    public int bindingIndex = 0;

    [Header("UI References")]
    public Button rebindButton;
    public Image bindingIconImage;
    public TMP_Text bindingDisplayText;
    public KeyIconDatabase iconDatabase;
    public GameObject waitingOverlay;
    public TMP_Text statusTipText;

    private void OnEnable()
    {
        UpdateUI();
        rebindButton.onClick.AddListener(OnRebindButtonClicked);
    }

    private void OnDisable()
    {
        rebindButton.onClick.RemoveListener(OnRebindButtonClicked);
    }

    private void OnRebindButtonClicked()
    {
        // hint when enter listening
        rebindButton.interactable = false;
        bindingDisplayText.text = "監聽中......";
        if (bindingIconImage != null) bindingIconImage.enabled = false;
        if (waitingOverlay != null) waitingOverlay.SetActive(true);
        if (statusTipText != null) statusTipText.text = "請按下想綁定的按鍵 (按 ESC 取消)";

        // start rebind
        GameInputManager.Instance.StartRebind(
            actionReference,
            bindingIndex,
            onComplete: () =>
            {
                FinishRebind("綁定成功！");
            },
            onCancel: () =>
            {
                FinishRebind("已取消改鍵");
            },
            onDuplicate: () =>
            {
                FinishRebind("此按鍵已被使用，已還原設定！");
            }
        );
    }

    private void FinishRebind(string message)
    {
        rebindButton.interactable = true;
        if (waitingOverlay != null) waitingOverlay.SetActive(false);
        if (statusTipText != null) statusTipText.text = message;
        bindingDisplayText.text = "";
        UpdateUI();
    }

    public void UpdateUI()
    {
        if(iconDatabase == null || bindingIconImage == null || actionReference == null) return;

        var bindings = actionReference.action.bindings;
        if(bindingIndex < 0 || bindingIndex >= bindings.Count)return;

        string path = bindings[bindingIndex].effectivePath;
        Sprite icon = iconDatabase.GetIcon(path);

        bindingIconImage.sprite = icon;
        bindingIconImage.enabled = icon != null;
    }
}