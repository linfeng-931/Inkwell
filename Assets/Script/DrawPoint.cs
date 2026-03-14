using UnityEngine;

public class DrawPoint : MonoBehaviour
{
    public GameObject circle;
    public bool isAct;
    public Sprite[] drawPoint;

    private float angle;
    private float rotateSpeed = 10f;
    private GameObject effect;
    private bool changeStatus;
    private int newStatus;
    private int status;
    private bool finishChangeStatus;
    private int changeStatusStep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        angle = 0f;
        effect = circle.transform.GetChild(0).gameObject;
        isAct = false;
        changeStatus = false;
        status = 0;
        finishChangeStatus = false;
        changeStatusStep = 0;
        newStatus = 0;
    }

    // Update is called once per frame
    void Update()
    {
        CircleRotate();
        ChangeStatus();

        if(isAct){
            effect.SetActive(true);
            newStatus = 1;
            changeStatus = true;
        }
        else{
            effect.SetActive(false);
            newStatus = 0;
            changeStatus = true;
        }
    }

    void CircleRotate()
    {
        angle += Time.deltaTime * rotateSpeed;
        if(angle >= 360) angle -= 360f;
        circle.transform.rotation = Quaternion.Euler(0, 0, angle);;
    }

    void ChangeStatus()
    {
        if(!changeStatus) return;

        Transform circleTrans = circle.transform;
        if(status == newStatus && circleTrans.localScale.x == 0.13f) return;

        switch (changeStatusStep)
        {
            case 0:
                if(circleTrans.localScale.x <= 0.05f)
                {
                    circleTrans.localScale = new Vector3(0.05f, 0.05f, 1);
                    circle.GetComponent<SpriteRenderer>().sprite = drawPoint[newStatus];
                    changeStatusStep++;
                    return;
                }
                circleTrans.localScale -= new Vector3(1, 1, 0) * Time.deltaTime;
                break;
            case 1:
                if(circleTrans.localScale.x >= 0.13)
                {
                    circleTrans.localScale = new Vector3(0.13f, 0.13f, 1);
                    changeStatusStep++;
                    finishChangeStatus = true;
                }
                else circleTrans.localScale += new Vector3(1, 1, 0) * Time.deltaTime;
                break;
            default:
                break;
        }

        if (finishChangeStatus)
        {
            changeStatus = false;
            changeStatusStep = 0;
            finishChangeStatus = false;
            status = newStatus;
        }
    }
}
