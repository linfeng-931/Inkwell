using UnityEngine;

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
    public GameObject[] subPages;

    private float timer;

    void Start()
    {
        majorIndex = 0;
        lastIndex = 0;
        changeLastMark = false;
        changeMainMark = false;
        timer = 0f;
    }

    void Update()
    {
        if (lastIndex!=majorIndex)
        {
            timer += Time.unscaledDeltaTime;
            if(timer >= marks[majorIndex].switchLayerDelay)
            {
                marks[majorIndex].transform.SetParent(ActMark.transform, false);
                changeMainMark = true;
            }

            if(timer >= marks[lastIndex].switchLayerDelayClose)
            {
                marks[lastIndex].transform.SetParent(disActMark.transform, false);
                changeLastMark = true;
            }

            if(changeLastMark && changeMainMark)
            {
                marks[lastIndex].setAct(false);
                lastIndex = majorIndex;
                timer = 0f;
                changeLastMark = false;
                changeMainMark = false;
            }
        }
    }
}
