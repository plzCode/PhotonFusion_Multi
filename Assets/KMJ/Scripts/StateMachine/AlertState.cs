using UnityEngine;
using UnityEngine.InputSystem.XR;
using Zombie.States;

public class AlertState : ZombieState
{
    const float REPATH = 0.25f;
    float elapsed;

    public AlertState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.IsAlert = true;
        ctrl.SetMoveSpeed(1f);
        elapsed = REPATH;
    }

    public override void Update()
    {
        if (ctrl.Target == null)
        {
            ctrl.ChangeState(new IdleState(ctrl));
            return;
        }

        elapsed += Time.deltaTime;
        if (elapsed >= REPATH)
        {
            elapsed = 0;
            ctrl.agent.SetDestination(ctrl.Target.position); // 계속 추적
        }
    }

    //public override void Exit() => ctrl.IsAlert = false; // ★ Bool OFF
}
