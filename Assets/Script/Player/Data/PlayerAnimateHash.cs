using UnityEngine;

public class PlayerAnimateHash
{
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Walk = Animator.StringToHash("Walk");
    public static readonly int Run = Animator.StringToHash("Run2");
    public static readonly int JumpStart = Animator.StringToHash("Jump_1");
    public static readonly int JumpEnd = Animator.StringToHash("Jump_1_end");
    public static readonly int Fall = Animator.StringToHash("NormalFall");
    public static readonly int DoubleJumpStart = Animator.StringToHash("Jump_2");
    public static readonly int DoubleJumpEnd = Animator.StringToHash("Jump_2_end");

    // dash
    public static readonly int Dash = Animator.StringToHash("Dash");
    public static readonly int AirDash = Animator.StringToHash("AirDash");

    // hurt and deadth
    public static readonly int Hurt = Animator.StringToHash("Damadged");
    public static readonly int StrongFall = Animator.StringToHash("StrongFall");
    public static readonly int Death = Animator.StringToHash("Death");
}
