using UnityEngine;
using Zombie.States;

public class ChaseState : ZombieState
{
    public ChaseState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.anim.SetFloat("Speed", 1f);
        ctrl.agent.isStopped = false;
        Vector3 goal = ctrl.Target ? ctrl.Target.position : ctrl.pendingGoal;
        ctrl.agent.SetDestination(goal);
        ctrl.zCtrl.SfxChase();
    }

    public override void Update()
    {
        // 1) 타깃 찾아서 추적
        if (ctrl.Target)
        {
            ctrl.agent.SetDestination(ctrl.Target.position);
            return;
        }

        // 2) 타깃 없으면 웨이브 epicenter 로 이동
        ctrl.agent.SetDestination(ctrl.pendingGoal);

        // 3) 도착 후에도 타깃 없으면 IdleWalk 복귀
        if (!ctrl.agent.pathPending && ctrl.agent.remainingDistance < 0.5f)
            ctrl.ChangeState(new IdleWalkState(ctrl));
    }
}