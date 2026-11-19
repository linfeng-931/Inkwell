using UnityEngine;

/// <summary>
/// 腳步聲：每一幀依「現在的狀況」決定要不要播，而不是靠狀態的 Enter / Exit。
/// 這樣高低差造成的狀態短暫切換、或是別的地方不小心停掉腳步聲，都不會讓聲音卡頓或一直消失。
/// 掛在 Player 上。使用後請把 WalkState / RunState 裡原本播放、停止腳步聲的程式拿掉。
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Tooltip("離開地面後，還要持續播放多久（秒）。避免走在階梯、小高低差時 isGrounded 閃一下就斷音")]
    [SerializeField] private float groundGraceTime = 0.12f;

    [Tooltip("水平速度低於這個值視為沒有在移動，不播腳步聲")]
    [SerializeField] private float minSpeedX = 0.1f;

    private PlayerController player;
    private float lastGroundedTime = -100f;
    private bool isPlaying;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void OnDisable()
    {
        StopFootsteps();
    }

    private void Update()
    {
        if (player.isGrounded) lastGroundedTime = Time.time;

        bool inMoveState = player.currentState is WalkState || player.currentState is RunState;
        bool groundedRecently = Time.time - lastGroundedTime <= groundGraceTime;
        bool moving = player.rig != null && Mathf.Abs(player.rig.linearVelocity.x) > minSpeedX;

        bool shouldPlay = inMoveState && groundedRecently && moving
                          && player.footstepClip != null && player.audioManager != null;

        if (shouldPlay == isPlaying) return;

        if (shouldPlay) StartFootsteps();
        else StopFootsteps();
    }

    private void StartFootsteps()
    {
        isPlaying = true;
        player.audioManager.PlayLoop(player.footstepClip, LoopChannel.Footstep);
    }

    private void StopFootsteps()
    {
        isPlaying = false;
        if (player != null && player.audioManager != null)
        {
            player.audioManager.StopLoop(LoopChannel.Footstep);
        }
    }
}
