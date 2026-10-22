using UnityEngine;
using UnityEngine.EventSystems;

public class NotePresenter : MonoBehaviour
{
    [SerializeField] private NoteView view;
    [SerializeField] private ItemIntroViewForNote itemIntroViewForNote;
    private CanvasGroup viewCanvasGroup;
    private NoteModel model;

    // for story
    public bool canOpenNote {get; set;} = true;

    private void Awake()
    {
        viewCanvasGroup = view.GetComponent<CanvasGroup>();
        model = new NoteModel();
        view.Initialize(this);
    }

    private void Start()
    {
        model.Initialize(view.GetSubPageAmounts());
        model.majorIndex = 0; // set default page (0 = inventory)
        view.InitDefaultState(model.majorIndex, model.GetCurrentSubPage(), model.subPageAmount[model.majorIndex]);
        view.OnExit();
    }

    private void OnEnable()
    {
        GameEvent.OnToggleNote += ToggleNote;
    }

    private void OnDisable()
    {
        GameEvent.OnToggleNote -= ToggleNote;
    }

    public void ToggleNote()
    {
        if(!canOpenNote) return;
        
        if (viewCanvasGroup.alpha > 0)
        {
            UIManager.Instance.CloseTopPanel();
            itemIntroViewForNote.RefreshItemPage();
            GameEvent.OnInteractStateChanged.Invoke(false);
            GameEvent.OnNoteStateChanged?.Invoke(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            UIManager.Instance.OpenPanel(view);
            GameEvent.OnInteractStateChanged.Invoke(true);
            GameEvent.OnNoteStateChanged?.Invoke(true);
        }
    }

    public void OnMarkClicked(int newIndex)
    {
        if(model.majorIndex == newIndex || view.isAnimating) return;

        int lastIndex = model.majorIndex;
        model.majorIndex = newIndex;

        bool isFlipLeft = lastIndex < newIndex;
        view.SwitchMajorPage(lastIndex, newIndex, model.GetCurrentSubPage(), model.subPageAmount[newIndex], isFlipLeft);
    }

    public void OnArrowClicked(bool isLeft)
    {
        if(view.isAnimating) return;

        int currentSubPage = model.GetCurrentSubPage();
        int lastSubPage = currentSubPage;

        if(isLeft && currentSubPage > 0)
        {
            currentSubPage--;
        }
        else if(!isLeft && currentSubPage < model.subPageAmount[model.majorIndex] - 1)
        {
            currentSubPage++;
        }

        if(currentSubPage == lastSubPage) return;

        model.subPageIndexMemory[model.majorIndex] = currentSubPage;

        view.SwitchSubPage(model.majorIndex, lastSubPage, currentSubPage, model.subPageAmount[model.majorIndex]);
    }
}
