using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MouseInput;

public class BulletSkill : MonoBehaviour
{
    public int actButton = 0;
    public float maxScale;
    public float minScale;
    public float enlargeSpeed = 1f;
    public Sprite[] sprites;
    public GameObject bulletPrefab;

    private bool isAct; 
    private bool startTimer;
    private int targetStyle;
    private bool enlargeFlag;
    private Vector2 mousePos;
    private float timer;
    private List<GameObject> drawMeshes;
    private SpriteRenderer spriteRenderer;
    private bool isShoot;
    private Vector3 target;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isAct = false;
        startTimer = false;
        targetStyle = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        enlargeFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAct)
        {
            if (Input.GetMouseButtonDown(actButton))
            {
                mousePos = Mouse.current.position.value;
                startTimer = true;
            }
            if (startTimer)
            {
                timer+=Time.deltaTime;
                if(Vector2.Distance(Mouse.current.position.value, mousePos) > 10f)
                {
                    timer = 0f;
                    startTimer = false;
                }
                if(timer > 1f)
                {
                    isAct = true;
                    targetStyle = 1;
                    spriteRenderer.enabled = true;
                    startTimer = false;
                    timer = 0;
                    drawMeshes = new List<GameObject>(GameObject.FindGameObjectsWithTag("DrawMesh"));
                    foreach(GameObject drawMesh in drawMeshes)
                    {
                        DrawMesh drawMesh1 = drawMesh.GetComponent<DrawMesh>();
                        if (!drawMesh1.isAct)
                        {
                            drawMesh1.DrawMeshDestory();
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(actButton))
            {
                isShoot = true;
                targetStyle = 3;
                enlargeFlag = false;
                transform.GetChild(0).gameObject.SetActive(false);
                target = MouseUtils.GetMouseWorldPosition();
            }
        }
        if(targetStyle!=3) transform.position = MouseUtils.GetMouseWorldPosition();
        TargetPointStyle();
    }

    private void TargetPointStyle()
    {
        if(targetStyle == 0) return;

        switch (targetStyle)
        {
            case 1:
                transform.localScale += new Vector3(1, 1, 0)*enlargeSpeed*Time.deltaTime*20f;
                if(transform.localScale.x >= maxScale){
                    transform.localScale = new Vector3(maxScale, maxScale, 1f);
                    targetStyle++;
                    transform.GetChild(0).gameObject.SetActive(true);
                }
                break;
            case 2:
                if (!enlargeFlag)
                {
                    transform.localScale -= new Vector3(1, 1, 0)*enlargeSpeed*Time.deltaTime;
                    if(transform.localScale.x <= minScale){
                        transform.localScale = new Vector3(minScale, minScale, 1f);
                        enlargeFlag = true;
                    }
                }
                else
                {
                    transform.localScale += new Vector3(1, 1, 0)*enlargeSpeed*Time.deltaTime;
                    if(transform.localScale.x >= maxScale){
                        transform.localScale = new Vector3(maxScale, maxScale, 1f);
                        enlargeFlag = false;
                    }
                }
                break;
            case 3:
                if (!enlargeFlag)
                {
                    transform.localScale -= new Vector3(1, 1, 0)*enlargeSpeed*Time.deltaTime*20f;
                    if(transform.localScale.x <= 0.1f){
                        transform.localScale = new Vector3(0.1f, 0.1f, 1f);
                        enlargeFlag = true;
                        spriteRenderer.sprite = sprites[1];
                        transform.GetChild(1).gameObject.SetActive(true);
                    }
                }
                else
                {
                    if(isShoot){
                        GameObject bullet = Instantiate(bulletPrefab);
                        bullet.GetComponent<Bullet>().target = new Vector3(target.x, target.y, -1f);
                        isShoot = false;
                    }
                    transform.localScale += new Vector3(1, 1, 0)*enlargeSpeed*Time.deltaTime*20f;

                    Color c = spriteRenderer.color;
                    c.a -= Time.deltaTime*4f;
                    c.a = Mathf.Max(0, c.a);
                    spriteRenderer.color = c;

                    if(transform.localScale.x >= maxScale*2f){
                        transform.localScale = new Vector3(maxScale*2f, maxScale*2f, 1f);
                        enlargeFlag = false;
                        targetStyle = 0;
                        spriteRenderer.enabled = false;
                        isAct = false;
                        spriteRenderer.sprite = sprites[0];
                        c.a = 1;
                        spriteRenderer.color = c;
                        transform.GetChild(1).gameObject.SetActive(false);
                    }
                }
                break;
            default:
                break;
        }
    }
}
