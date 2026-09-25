using UnityEngine;

public class FadeView : BasePanel
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayFadeIn()
    {
        animator.Play("ChangeScene_start");
    }

    public void PlayFadeOut()
    {
        animator.Play("ChangeScene_end");
    }
}
