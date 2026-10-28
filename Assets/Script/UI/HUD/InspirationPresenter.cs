using System.Collections.Generic;
using UnityEngine;

public class InspirationPresenter : MonoBehaviour
{
    [SerializeField] private InspirationView view;

    public int currentValue {get; private set;}

    void Start()
    {
        currentValue = SaveManager.Instance.currentSave.note.items.GetValueOrDefault("inspiration", 0);
        view.Initialize(currentValue);
        view.OnExit();
    }

    private void OnEnable()
    {
        GameEvent.OnInspirationCollected += HandleTextOpened;
    }

    private void OnDisable()
    {
        GameEvent.OnInspirationCollected -= HandleTextOpened;
    }

    private void HandleTextOpened(int value)
    {
        currentValue += value;
        SaveManager.Instance.currentSave.note.items["inspiration"] = currentValue;

        if (!view.IsRunning)
        {
            UIManager.Instance.OpenPanel(view);
        }

        view.UpdateAmountText(currentValue);
    }
}
