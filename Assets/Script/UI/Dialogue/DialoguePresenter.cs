using UnityEngine;

public class DialoguePresenter : MonoBehaviour
{
    [SerializeField] private DialogueView largeView;
    [SerializeField] private WorldDialogueView mediumView;

    [Header("Game Object Visual")]
    [SerializeField] private GamePlayHUD gamePlayHUD; // Player UI
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerHealth playerHealth;

    private IDialogueView currentView;
    private DialogueType currentType;

    void Start()
    {
        largeView.OnExit();
        mediumView.Close();
    }

    private void OnEnable()
    {
        GameEvent.OnDialogueOpened += HandleDialogueOpened;
        GameEvent.OnDialogueLineUpdated += HandleLineUpdated;
        GameEvent.OnDialogueClosed += HandleDialogueClosed;
        GameEvent.OnDialogueNextPressed += HandleNextPressed;
    }

    private void OnDisable()
    {
        GameEvent.OnDialogueOpened -= HandleDialogueOpened;
        GameEvent.OnDialogueLineUpdated -= HandleLineUpdated;
        GameEvent.OnDialogueClosed -= HandleDialogueClosed;
        GameEvent.OnDialogueNextPressed -= HandleNextPressed;
    }

    private void HandleDialogueOpened(DialogueType type, Transform target)
    {
        currentType = type;

        if (type == DialogueType.Large)
        {
            currentView = largeView;
            HidePlayerUI();
            UIManager.Instance.OpenPanel(largeView);
        }
        else if (type == DialogueType.Medium)
        {
            currentView = mediumView;
            mediumView.Open(target);
        }
    }
    
    private void HandleLineUpdated(string text)
    {
        currentView?.UpdateDialogueText(text);
    }

    private void HandleNextPressed()
    {
        if (currentView == null) return;
        if (Time.time - currentView.StartTypingTime < 0.15f) return;

        if (currentView.IsTyping)
        {
            currentView.CompleteText();
        }
        else
        {
            DialogueManager.Instance.NextLine();
        }
    }

    private void HandleDialogueClosed()
    {
        if (currentType == DialogueType.Large)
        {
            UIManager.Instance.CloseTopPanel();
            ShowPlayerUI();
        }
        else if (currentType == DialogueType.Medium)
        {
            mediumView.Close();
        }

        currentView = null;
    }

    private void HidePlayerUI()
    {
        gamePlayHUD.enabled = false;
        playerEnergy.enabled = false;
        playerHealth.enabled = false;
        gamePlayHUD.gameObject.SetActive(false);
    }

    private void ShowPlayerUI()
    {
        // Show Player UI
        gamePlayHUD.gameObject.SetActive(true);
        gamePlayHUD.enabled = true;
        playerEnergy.enabled = true;
        playerHealth.enabled = true;
    }
}
