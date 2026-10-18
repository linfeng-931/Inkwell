using UnityEngine;

public class ItemIntroPresenter : MonoBehaviour
{
    [SerializeField] private ItemIntroView view;

    void Start()
    {
        view.OnExit();
    }

    private void OnEnable()
    {
        GameEvent.OnItemIntroOpened += HandleItemIntroOpened;
    }

    private void OnDisable()
    {
        GameEvent.OnItemIntroOpened -= HandleItemIntroOpened;
    }

    private void HandleItemIntroOpened()
    {
        view.UpdateItemIntroText(ItemManager.Instance.currentItem);
        UIManager.Instance.OpenPanel(view);
    }
}
