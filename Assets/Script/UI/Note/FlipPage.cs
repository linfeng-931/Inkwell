using System.Runtime.CompilerServices;
using UnityEngine;

public class FlipPage : MonoBehaviour
{
    public DissolveEffect dissolveEffect;
    public bool canFlip;

    private float timer;
    private Animator ani;
    private bool turnLeft;
    private bool turnRight;

    void Start()
    {
        timer = 0f;
        canFlip = true;
        ani = GetComponent<Animator>();
    }
    void Update()
    {
        if (!canFlip)
        {
            if(dissolveEffect.canAct)
            {
                canFlip = true;
                timer = 0f;
            }
        }

        if(turnLeft || turnRight)
        {
            timer+=Time.unscaledDeltaTime;
            if(timer > 0.38f)
            {
                if (turnLeft)
                {
                    ani.SetTrigger("left");
                    turnLeft = false;
                }
                else
                {
                    ani.SetTrigger("right");
                    turnRight = false;
                }
                timer = 0f;
            }
        }  
    }

    public void Flip(bool isLeft)
    {
        if(!canFlip) return;

        if (isLeft)
        {
            turnLeft = true;
        }
        else
        {
            turnRight = true;
        }
        canFlip = false;
    }
}
