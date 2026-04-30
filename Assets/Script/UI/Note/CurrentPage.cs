using UnityEngine;
using UnityEngine.UIElements;

public class CurrentPage : MonoBehaviour
{
    [Header("Mark")]
    public int majorIndex;
    public GameObject disActMark;
    public GameObject ActMark;
    public Mark[] marks;

    private int lastIndex;
    private bool changeMainMark;
    private bool changeLastMark;

    [Header("SubPage")]
    public int subPageIndex;
    public GameObject[] subPages;
    public GameObject ArrowL;
    public GameObject ArrowR;
    public float subPageDelayTime;

    private int[] subPageAmount;
    private int currentSubPage;
    private int lastSubPage;
    private bool changingSubPage;
    private bool changingSubPage1;
    private bool cm;
    private bool cm2;
    private int[] subPageIndexMemory;

    private float timer;
    private float timer1;
    

    void Start()
    {
        majorIndex = 0;
        lastIndex = 0;
        changeLastMark = false;
        changeMainMark = false;
        timer = 0f;
        subPageAmount = new int[subPages.Length];
        for(int i = 0; i< subPageAmount.Length; i++)
        {
            subPageAmount[i] = subPages[i].transform.childCount;
        }
        currentSubPage = 0;
        lastSubPage = 0;
        changingSubPage = true;
        changingSubPage1 = false;
        timer1 = 0f;
        cm = false;
        cm2 = false;
        subPageIndexMemory = new int[4]{0,0,0,0};
    }

    void Update()
    {
        //Change Main Mark
        if (lastIndex!=majorIndex)
        {
            timer += Time.unscaledDeltaTime;
            if(timer >= marks[majorIndex].switchLayerDelay)
            {
                if(subPageIndexMemory[majorIndex] == 0) marks[majorIndex].transform.SetParent(ActMark.transform, false);
                subPages[majorIndex].SetActive(true);
                changeMainMark = true;
            }

            if(timer >= marks[lastIndex].switchLayerDelayClose)
            {
                marks[lastIndex].transform.SetParent(disActMark.transform, false);
                subPages[lastIndex].SetActive(false);
                changeLastMark = true;
            }

            if(changeLastMark && changeMainMark)
            {
                marks[lastIndex].setAct(false);
                lastIndex = majorIndex;
                timer = 0f;
                changeLastMark = false;
                changeMainMark = false;
                changingSubPage = true;
                subPageIndex = subPageIndexMemory[majorIndex];
                currentSubPage = subPageIndex;
                changingSubPage1 = true;
            }
        }

        //Change SubPage
        if(changingSubPage)
        {
            //Arrow Control
            if(currentSubPage == 0)
            {
                ArrowL.SetActive(false);
            }
            if(currentSubPage != 0)
            {
                ArrowL.SetActive(true);
            }
            if(currentSubPage == subPageAmount[majorIndex]-1)
            {
                ArrowR.SetActive(false);
            }
            if(currentSubPage == subPageAmount[majorIndex]-2)
            {
                ArrowR.SetActive(true);
            }

            //Mark Control
            if((lastSubPage == 0 && currentSubPage > 0) || cm)
            {
                timer += Time.unscaledDeltaTime;
                cm = true;
                if(timer >= marks[majorIndex].switchLayerDelayClose)
                {
                    marks[majorIndex].transform.SetParent(disActMark.transform, false);
                    timer = 0f;
                    changingSubPage = false;
                    cm = false;
                }
            }
            else if((lastSubPage == 1 && currentSubPage == 0) || cm2)
            {
                timer += Time.unscaledDeltaTime;
                cm2 = true;
                if(timer >= marks[majorIndex].switchLayerDelay)
                {
                    marks[majorIndex].transform.SetParent(ActMark.transform, false);
                    timer = 0f;
                    changingSubPage = false;
                    cm2 = false;
                }
            }
            else changingSubPage = false;
        } 

        //Pages Control
        if (changingSubPage1)
        {
            timer1 += Time.unscaledDeltaTime;
            if(timer1 >= subPageDelayTime)
            {
                Transform main = subPages[majorIndex].transform.GetChild(currentSubPage);
                Transform last = subPages[majorIndex].transform.GetChild(lastSubPage);
                last.gameObject.SetActive(false);
                main.gameObject.SetActive(true);
                lastSubPage = currentSubPage;

                timer1 = 0f;
                changingSubPage1 = false;
            }
        }
    }

    public void UpdateSubPageAmount(int changeObjIndex)
    {
        subPageAmount[changeObjIndex] = subPages[changeObjIndex].transform.childCount;
    }

    public void ChangeSubPage(bool isLeft)
    {
        if(changingSubPage) return;

        changingSubPage = true;
        changingSubPage1 = true;

        if(isLeft) currentSubPage--;
        else currentSubPage++;

        subPageIndexMemory[majorIndex] = currentSubPage;
    }
}
