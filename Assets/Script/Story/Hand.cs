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
    private Vector3 handEndPos = new Vector3(27.421f, 6.54f, 12);
    private bool handReady;
    private Animator handAni;
    private float timer;

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
            hand.transform.position = Vector3.MoveTowards(hand.transform.position, handEndPos+Vector3.down*10f, Time.deltaTime*3f);
            if(timer > 0.1f) Time.timeScale = 0.3f;
            if(timer > 0.7f)
            {
                foreach(Rigidbody rig in buildingRig)
                {
                    rig.useGravity = true;
                    rig.isKinematic = false;
                }
                playerController.enabled = false;
                player.GetComponent<Rigidbody>().linearVelocity = Vector3.down * 5f;
                playerAnimator.SetInteger("action", 7);
            }
        }
        if (isAct)
        {   
            float dis = Math.Abs(player.transform.position.x - targetX);
            if(dis < 5f && step == 0)
            {
                particleSystemHole.Play();
                step++;
            }
            if(dis < 3.5f && step == 1)
            {
                ParticleRotate();
            }
            if(step == 2)
            {
                hand.transform.position = Vector3.MoveTowards(hand.transform.position, handEndPos, Time.deltaTime*10f);
                if(Vector3.Distance(hand.transform.position, handEndPos) <= 0.1f)
                {
                    handReady = true;
                    hand.transform.position = handEndPos;
                    handAni.speed = 1;
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

    void ParticleRotate()
    {
        particleSystemHole.transform.Rotate(0, 90f * Time.deltaTime, 0);

        if (particleSystemHole.transform.localEulerAngles.y >= 90f && particleSystemHole.transform.localEulerAngles.y < 100f)
        {
            particleSystemHole.transform.localEulerAngles = new Vector3(-90f, 90f, 0);

            var vel = particleSystemHole.velocityOverLifetime;
            vel.enabled = true;

            hand.SetActive(true);
            step++;
        }
    }
}
