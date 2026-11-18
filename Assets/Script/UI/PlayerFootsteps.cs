using UnityEngine;
public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private float groundGraceTime = 0.12f;
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