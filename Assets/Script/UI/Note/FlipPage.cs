using UnityEngine;
using UnityEngine.UI;

public class FlipPage : MonoBehaviour
{
    private Animator ani;
    private Image image;

    void Awake()
    {
        ani = GetComponent<Animator>();
        image = GetComponent<Image>();
    }

    public void Flip(bool isLeft)
    {
        if (isLeft)
        {
            ani.SetTrigger("left");
        }
        else
        {
            ani.SetTrigger("right");
        }
    }

    public void DisableImage()
    {
        image.enabled = false;
    }

    public void EnableImage()
    {
        image.enabled = true;
    }
}
