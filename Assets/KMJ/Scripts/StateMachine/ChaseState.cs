using UnityEngine;
using Zombie.States;

public class ChaseState : ZombieState
{
    public ChaseState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.zCtrl.SfxChase();
        if (ctrl.agent.enabled)
        {
            ctrl.anim.SetFloat("Speed", 1f);      // 달리기 BlendTree
            ctrl.agent.isStopped = false;
        }
        Vector3 goal = ctrl.Target ? ctrl.Target.position : ctrl.pendingGoal;
        if (ctrl.agent.enabled)
        {
            ctrl.agent.SetDestination(goal);

        }
        
    }

    public override void Update()
    {
        if (ctrl.Target != null && ctrl.InAttackRange && !ctrl.IsAttacking)
        {
            Debug.Log("[ChaseState] 사거리 진입 → AttackState");
            ctrl.ChangeState(new AttackState(ctrl));
            return;
        }
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
    public override void Exit()
    {
        Debug.Log("[ChaseState] Exit");
    }
}