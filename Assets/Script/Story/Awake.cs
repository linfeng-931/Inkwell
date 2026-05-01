using UnityEngine;

public class Awake : MonoBehaviour
{
    private PlayerController playerController;
    private float timer;
    private Animator animator;

    void Start()
    {
        timer = 0f;
        animator = GetComponent<Animator>();
        animator.speed = 0;
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerController.enabled = false;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer > 1.5f)
        {
            animator.speed = 1;
        }
    }
    
    public void EndAwake()
    {
        GameObject playerFace = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).gameObject;
        playerFace.SetActive(true);
        gameObject.SetActive(false);
        playerController.enabled = true;
    }
}
