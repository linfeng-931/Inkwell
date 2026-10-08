using Unity.Cinemachine;
using UnityEngine;

public class DeathState : PlayerState
{
    private float stateStartTime;
    private bool hasRelocated = false;
    private bool hasTriggeredFadeOut = false;
    private int originalLayer;

    public DeathState(PlayerController manager) : base(manager) { }

    public override void Enter()
    {
        base.Enter();
        stateStartTime = Time.time;
        hasRelocated = false;
        hasTriggeredFadeOut = false;

        // 1. 關閉碰撞避免被鞭屍
        if (manager.col != null)
        {
            manager.col.enabled = false;
        }

        // 2. 避免敵人射線/攻擊鎖定倒地玩家
        originalLayer = manager.gameObject.layer;
        manager.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        manager.animator.Play(PlayerAnimateHash.Dead, 0, 0f);
        manager.rig.linearVelocity = Vector3.zero;
        manager.canTurn = false;
        manager.isPlayerInputEnabled = false;
    }

    public override void Update()
    {
        base.Update();

        float elapsedTime = Time.time - stateStartTime;

        // 黑幕淡出
        if (!hasTriggeredFadeOut && elapsedTime >= manager.fadeOutWaitTime)
        {
            hasTriggeredFadeOut = true;
            GameEvent.OnToggleFade(false);
        }

        // 移回存檔點 (黑幕蓋住畫面後)
        if (!hasRelocated && elapsedTime >= manager.fadeOutWaitTime + 1.0f)
        {
            hasRelocated = true;

            // 抬高 0.2f 避免直接陷進地面碰撞體內部導致判定穿模
            Vector3 safeTarget = manager.lastCheckpointPosition + Vector3.up * 0.2f;

            // 使用 PlayerController 統整的 TeleportTo 清空速度並定位
            manager.TeleportTo(safeTarget, true);

            // 【關鍵】在位置確定後，立刻開啟 Collider 並刷新物理
            if (manager.col != null)
            {
                manager.col.enabled = true;
            }
            Physics.SyncTransforms();

            // 重置相機
            if (manager.mainCam != null && manager.mainCam.GetComponent<CinemachineBrain>().ActiveVirtualCamera is CinemachineVirtualCameraBase vcam)
            {
                vcam.PreviousStateIsValid = false;
            }

            manager.animator.Play(PlayerAnimateHash.Idle, 0, 0f);
            GameEvent.OnToggleFade(true);
        }

        // 結束死亡狀態，切回 Idle
        if (elapsedTime >= manager.fadeOutWaitTime + 3f)
        {
            manager.TransitionToState<IdleState>();
        }
    }

    public override void Exit()
    {
        base.Exit();

        // 恢復原始圖層與操作
        manager.gameObject.layer = originalLayer;
        if (manager.col != null) manager.col.enabled = true;

        manager.isPlayerInputEnabled = true;
        manager.canTurn = true;
    }
}