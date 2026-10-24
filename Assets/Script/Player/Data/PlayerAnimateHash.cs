using UnityEngine;

public class PlayerAnimateHash
{
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Walk = Animator.StringToHash("Walk");
    public static readonly int Run = Animator.StringToHash("Run");
    public static readonly int JumpStart = Animator.StringToHash("Jump1");
    public static readonly int JumpEnd = Animator.StringToHash("Landing");
    public static readonly int Fall = Animator.StringToHash("NormalFall");
    public static readonly int DoubleJumpStart = Animator.StringToHash("Jump2");
    public static readonly int DoubleJumpEnd = Animator.StringToHash("Landing");
    public static readonly int HoldingPen = Animator.StringToHash("HoldingPen");
    public static readonly int Roll = Animator.StringToHash("Roll");
    public static readonly int RollEnd = Animator.StringToHash("RollEnd");

    // dash
    public static readonly int Dash = Animator.StringToHash("Dash");
    public static readonly int AirDash = Animator.StringToHash("AirDash");

    // hurt and deadth
    public static readonly int Hurt = Animator.StringToHash("Damadged");
    public static readonly int StrongFall = Animator.StringToHash("StrongFall");
    public static readonly int Dead = Animator.StringToHash("Dead");
}
