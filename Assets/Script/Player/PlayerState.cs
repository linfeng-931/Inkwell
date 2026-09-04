/// <summary>
/// state regulation
/// </summary>
public abstract class PlayerState
{
    protected PlayerController manager;
    public PlayerState(PlayerController manager)
    {
        this.manager = manager;
    }

    /// <summary>
    /// init state, such as animation, audio
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// main logic/ change to other state
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// logic about rigbody
    /// </summary>
    public virtual void FixedUpdate() {}

    /// <summary>
    /// clear trush, such as animation, audio
    /// </summary>
    public virtual void Exit() { }
}