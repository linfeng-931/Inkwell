using System;
using System.Collections.Generic;

public class PlayerStateFactory
{
    private PlayerController context;

    //use dictionary control all states
    private Dictionary<Type, PlayerState> states = new Dictionary<Type, PlayerState>();

    public PlayerStateFactory(PlayerController currentContext)
    {
        //init manager
        context = currentContext;

        //generate states
        states[typeof(IdleState)] = new IdleState(context);
        states[typeof(WalkState)] = new WalkState(context);
        states[typeof(RunState)] = new RunState(context);
        states[typeof(AttackState)] = new AttackState(context);

        states[typeof(AirAttackState)] = new AirAttackState(context);
        states[typeof(JumpState)] = new JumpState(context);
        states[typeof(DoubleJumpState)] = new DoubleJumpState(context);
        states[typeof(FallState)] = new FallState(context);

        states[typeof(StrongFallState)] = new StrongFallState(context);
        states[typeof(DashState)] = new DashState(context);
        states[typeof(CutsceneState)] = new CutsceneState(context);
        states[typeof(HurtState)] = new HurtState(context);
        states[typeof(InteractState)] = new InteractState(context);
        states[typeof(HookState)] = new HookState(context);
    }

    /// <summary>
    /// get state by generic
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>state</returns>
    public PlayerState GetState<T>() where T : PlayerState
    {
        Type type = typeof(T);
        states.TryGetValue(type, out PlayerState state);
        return state;
    }
}
