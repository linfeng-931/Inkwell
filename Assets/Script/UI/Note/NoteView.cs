using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NoteView : BasePanel
{
    [Header("Marks")]
    [SerializeField] private Mark[] marks;
    [SerializeField] private Transform actMarkParent;
    [SerializeField] private Transform disActMarkParent;

    [Header("SubPages")]
    [SerializeField] private GameObject[] majorPages;
    [SerializeField] private GameObject arrowL;
    [SerializeField] private GameObject arrowR;
    [SerializeField] private float subPageDelayTime = 0.2f;

    [Header("Effects")]
    [SerializeField] private FlipPage flipPage;

    private NotePresenter notePresenter;
    public bool isAnimating { get; private set; }

    public void Initialize(NotePresenter presenter)
    {
        notePresenter = presenter;
        for (int i = 0; i < marks.Length; i++)
        {
            int index = i;
            marks[i].Init(index, () => presenter.OnMarkClicked(index));
        }

        arrowL.GetComponent<Button>().onClick.AddListener(() => presenter.OnArrowClicked(true));
        arrowR.GetComponent<Button>().onClick.AddListener(() => presenter.OnArrowClicked(false));
    }

    public void InitDefaultState(int defaultMajorIndex, int defaultSubPage, int maxSubPages)
    {
        for (int i = 0; i < majorPages.Length; i++)
        {
            bool isTargetMajor = i == defaultMajorIndex;
            
            majorPages[i].SetActive(isTargetMajor);
            
            marks[i].SetVisualAct(isTargetMajor);
            marks[i].transform.SetParent(isTargetMajor && defaultSubPage == 0 ? actMarkParent : disActMarkParent, false);

            for (int j = 0; j < majorPages[i].transform.childCount; j++)
            {
                majorPages[i].transform.GetChild(j).gameObject.SetActive(isTargetMajor && j == defaultSubPage);
            }
        }

        UpdateArrows(defaultSubPage, maxSubPages);
        
        isAnimating = false;
    }

    public int[] GetSubPageAmounts()
    {
        int[] amounts = new int[majorPages.Length];
        for (int i = 0; i < majorPages.Length; i++)
        {
            amounts[i] = majorPages[i].transform.childCount;
        }
        return amounts;
    }

    public void RefreshUI(int currentSubPage, int maxSubPages)
    {
        UpdateArrows(currentSubPage, maxSubPages);
    }

    public void SwitchMajorPage(int lastIndex, int newIndex, int targetSubPage, int maxSubPages, bool isFlipLeft)
    {
        StartCoroutine(MajorPageTransitionRoutine(lastIndex, newIndex, targetSubPage, maxSubPages, !isFlipLeft));
    }

    public void SwitchSubPage(int majorIndex, int lastSubPage, int currentSubPage, int maxSubPages)
    {
        StartCoroutine(SubPageTransitionRoutine(majorIndex, lastSubPage, currentSubPage, maxSubPages));
    }

    private IEnumerator MajorPageTransitionRoutine(int lastIndex, int newIndex, int targetSubPage, int maxSubPages, bool isFlipLeft)
    {
        isAnimating = true;

        marks[lastIndex].SetVisualAct(false);
        marks[newIndex].SetVisualAct(true);
        flipPage.Flip(isFlipLeft);

        // disable current page
        majorPages[lastIndex].SetActive(false); 

        // disable current mark
        yield return new WaitForSecondsRealtime(marks[lastIndex].switchLayerDelayClose);
        marks[lastIndex].transform.SetParent(disActMarkParent, false);

        // enable new mark
        yield return new WaitForSecondsRealtime(Mathf.Max(0, marks[newIndex].switchLayerDelay - marks[lastIndex].switchLayerDelayClose));
        if (targetSubPage == 0)
        {
            marks[newIndex].transform.SetParent(actMarkParent, false);
        }

        // enable new page
        yield return new WaitForSecondsRealtime(subPageDelayTime);
        majorPages[newIndex].SetActive(true);

        for (int i = 0; i < majorPages[newIndex].transform.childCount; i++)
        {
            majorPages[newIndex].transform.GetChild(i).gameObject.SetActive(i == targetSubPage);
        }

        UpdateArrows(targetSubPage, maxSubPages);
        isAnimating = false;
    }

    private IEnumerator SubPageTransitionRoutine(int majorIndex, int lastSubPage, int currentSubPage, int maxSubPages)
    {
        isAnimating = true;
        UpdateArrows(currentSubPage, maxSubPages);

        bool isFlipLeft = lastSubPage < currentSubPage;
        flipPage.Flip(!isFlipLeft);
        
        // disable current page
        Transform last = majorPages[majorIndex].transform.GetChild(lastSubPage);
        last.gameObject.SetActive(false); 

        // change mark layer
        if(lastSubPage == 0 && currentSubPage > 0)
        {
            yield return new WaitForSecondsRealtime(marks[majorIndex].switchLayerDelayClose);
            marks[majorIndex].transform.SetParent(disActMarkParent, false);
        }
        else if(lastSubPage == 1 && currentSubPage == 0)
        {
            yield return new WaitForSecondsRealtime(marks[majorIndex].switchLayerDelay);
            marks[majorIndex].transform.SetParent(actMarkParent, false);
        }
        else
        {
            // keep total waiting time
            yield return new WaitForSecondsRealtime(marks[majorIndex].switchLayerDelayClose);
        }

        // enable new page
        yield return new WaitForSecondsRealtime(subPageDelayTime);
        Transform current = majorPages[majorIndex].transform.GetChild(currentSubPage);
        current.gameObject.SetActive(true);

        isAnimating = false;
    }

    private void UpdateArrows(int currentSubPage, int maxSubPages)
    {
        arrowL.SetActive(currentSubPage > 0);
        arrowR.SetActive(currentSubPage < maxSubPages - 1);
    }
}
