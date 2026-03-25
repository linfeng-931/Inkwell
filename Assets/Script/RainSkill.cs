using UnityEngine;

public class RainSkill : MonoBehaviour
{
    public GameObject bwShader;
    public float shaderSpeed;
    public float shaderRadius;

    private bool isReady;
    private float readyTimer;
    private bool canShot;
    private bool cancelSkill;

    void Start()
    {
        isReady = false;
        canShot = false;
        cancelSkill = false;
        readyTimer = 0f;
    }

    void Update()
    {
        //Qkey ready skill
        if(!canShot && !cancelSkill)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                isReady = true;
                bwShader.SetActive(true);
            }
            if (isReady)
            {
                if (Input.GetKeyUp(KeyCode.Q))
                {
                    if(readyTimer > 1f){
                        canShot = true;
                    }
                    else cancelSkill = true;
                    isReady = false;
                    readyTimer = 0f;
                }

                readyTimer += Time.deltaTime;
                if(bwShader.transform.localScale.x <= shaderRadius)
                {
                    bwShader.transform.localScale += new Vector3(1f,0f,1f) * shaderSpeed * Time.deltaTime;
                }
            }
        }

        //shot
        if (canShot)
        {
             canShot = false;
        }

        //cancel Skill
        if (cancelSkill)
        {
            cancelSkill = false;
        }

    }
}
