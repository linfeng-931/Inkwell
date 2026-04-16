using UnityEngine;

public class PaperGround : MonoBehaviour
{
    private bool isAct;
    private float timer;
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0f;
        isAct = false;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAct)
        {
            timer += Time.deltaTime;
            if(timer > 2f)
            {
                animator.SetTrigger("isAct");
            }
            if(timer > 2.3f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isAct = true;
        }
    }
}
