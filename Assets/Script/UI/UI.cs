using UnityEngine;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    public bool isAct;

    [Header("Player Control")]
    public PlayerController playerController;
    public PlayerAni playerAni;
    public PlayerStatus playerStatus;
    public PlayerInput playerInput;

    [Header("UI Object")]
    public GameObject Note;


    private bool already;
    private bool noteIsAct;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isAct = false;
        already = false;
        noteIsAct = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isAct && !already) StartAct();
        if(!isAct && already) EndAct();
    }
    
    void StartAct()
    {
        playerController.enabled = false;
        playerAni.enabled = false;
        playerStatus.enabled = false;
        //playerInput更改（有需要的話再說）
        Time.timeScale = 0f;
        already = true;
    }

    void EndAct()
    {
        playerController.enabled = true;
        playerAni.enabled = true;
        playerStatus.enabled = true;
        Time.timeScale = 1f;
        already = false;
    }

    public void setAct(bool flag)
    {
        isAct = flag;
    }

    //UI Object Control
    public void NoteControl(InputAction.CallbackContext ctx)
    {
        if (noteIsAct)
        {
            isAct = false;
            noteIsAct = false;
            Note.SetActive(false);
        }
        else
        {
            isAct = true;
            noteIsAct = true;
            Note.SetActive(true);
        }
    }
}
