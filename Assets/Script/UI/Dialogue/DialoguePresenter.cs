using UnityEngine;

public class DialoguePresenter : MonoBehaviour
{
    [SerializeField] private DialogueView view;

    [Header("Game Object Visual")]
    [SerializeField] private GamePlayHUD gamePlayHUD; // Player UI
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerHealth playerHealth;


    void Start()
    {
        view.OnExit();
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

    private void HandleDialogueOpened()
    {
        HidePlayerUI();
        UIManager.Instance.OpenPanel(view);
    }
    
    private void HandleLineUpdated(string text)
    {
        view.UpdateDialogueText(text);
    }

    private void HandleNextPressed()
    {
        if (view.IsTyping)
        {
            if (Time.time - view.startTypingTime >= 0.15f){
                view.CompleteText();
            }
        }
        else
        {
            if (Time.time - view.startTypingTime >= 0.15f){
                DialogueManager.Instance.NextLine();
            }
        }
    }

    private void HandleDialogueClosed()
    {
        UIManager.Instance.CloseTopPanel();
        ShowPlayerUI();
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
