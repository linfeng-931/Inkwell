using System;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public Vector3 pos;
    public Rigidbody[] buildingRig;
    public GameObject hand;
    public ParticleSystem particleSystemHole;

    private bool isAct;
    private GameObject player;
    private PlayerController playerController;
    private Animator playerAnimator;
    private float targetX;
    private int step;
    private Vector3 handEndPos = new Vector3(26.89f, -2.2f, 1.37f);
    private bool handReady;
    private Animator handAni;
    private float timer;
    //private Vector3 handStartPos = new Vector3(26.89f, -2.2f, 7.83f);

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").gameObject;
        playerController = player.GetComponent<PlayerController>();
        playerAnimator = player.transform.GetChild(0).GetComponent<Animator>();
        targetX = pos.x;
        particleSystemHole.Stop();
        step = 0;
        handReady = false;
        handAni = hand.GetComponent<Animator>();
        handAni.speed = 0;
        hand.SetActive(false);
        timer = 0f;
    }

    
    void Update()
    {
        if (handReady)
        {
            timer += Time.deltaTime;
            if(timer > 0.15)
            {
                foreach(Rigidbody rig in buildingRig)
                {
                    rig.useGravity = true;
                }
                playerAnimator.SetInteger("action", 7);
            }
        }
        if (isAct)
        {   
            float dis = Math.Abs(player.transform.position.x - targetX);
            if(dis < 3.5f && step == 0)
            {
                particleSystemHole.Play();
                step++;
            }
            if(dis < 3f && step == 1)
            {
                var shape = particleSystemHole.shape;
                shape.rotation = new Vector3(90f, shape.rotation.y, 0f);
                step++;
                hand.SetActive(true);
            }
            if(step == 2)
            {
                hand.transform.position = Vector3.MoveTowards(hand.transform.position, handEndPos, Time.deltaTime*2f);
                if(Vector3.Distance(hand.transform.position, handEndPos) <= 0.1f)
                {
                    handReady = true;
                    hand.transform.position = handEndPos;
                    handAni.speed = 1;
                }
            }
            if(dis < 2.4f)
            {
                Time.timeScale = 0.5f;
                foreach(Rigidbody rig in buildingRig)
                {
                    rig.useGravity = true;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isAct = true;
            playerController.isInteract = true;
            playerController.SetUpForInteraction(new Vector3(targetX, 0, 0), 1);
        }
    }
}
