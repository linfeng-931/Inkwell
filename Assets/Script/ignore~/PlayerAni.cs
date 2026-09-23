using UnityEngine;

public class PlayerAni : MonoBehaviour
{
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PauseAni()
    {
        animator.speed = 0f;
    }
    public void ResumeAni()
    {
        animator.speed = 1f;
    }
}
