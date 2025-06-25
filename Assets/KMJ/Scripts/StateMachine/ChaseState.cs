using UnityEngine;
using Zombie.States;

public class ChaseState : ZombieState
{
    public ChaseState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.anim.SetFloat("Speed", 1f);          // Run 블렌드
        if (!ctrl.agent.enabled || !ctrl.agent.isOnNavMesh) return;
        ctrl.agent.isStopped = false;
        ctrl.agent.speed = ctrl.runSpeed;     // 2.5 m/s
    }

    public override void Update()
    {
        if (!ctrl.agent.enabled || !ctrl.agent.isOnNavMesh) return;

        if (!ctrl.Target)
        {
            ctrl.ChangeState(new IdleWalkState(ctrl));
            return;
        }

        if (ctrl.agent.enabled && ctrl.agent.isOnNavMesh)
            ctrl.agent.SetDestination(ctrl.Target.position);

        if (ctrl.InAttackRange)
            ctrl.ChangeState(new AttackState(ctrl));
        else if (!ctrl.InAlertRadius && !ctrl.InSightFov)
            ctrl.ChangeState(new IdleWalkState(ctrl));
    }

    public override void Exit() { }
}