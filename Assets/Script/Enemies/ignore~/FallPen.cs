using System;
using UnityEngine;

public class FallPen : MonoBehaviour
{
    public EnvironmentHazard environmentHazard;
    private Transform playerTrans;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Math.Abs(playerTrans.position.x - transform.position.x) < 0.2f)
        {
            animator.SetTrigger("start");
        }
        ReStart();
    }

    void ReStart()
    {
        if(!environmentHazard.reStart) return;
        animator.SetTrigger("reStart");
    }
}
