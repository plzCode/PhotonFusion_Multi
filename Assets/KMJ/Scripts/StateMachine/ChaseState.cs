using UnityEngine;
using Zombie.States;

public class ChaseState : ZombieState
{
    public ChaseState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.SetMoveSpeed(3.0f);            // Run 속도
        ctrl.anim.SetBool("IsAlert", true);
    }
    public override void Update()
    {
        if (ctrl.Target)
            ctrl.agent.SetDestination(ctrl.Target.position);

        if (ctrl.Target == null)
        {
            // 플레이어를 잃어버리면 Alert → Idle 로 복귀
            ctrl.ChangeState(new AlertState(ctrl));
            return;
        }

        // NavMeshAgent 가 초기화 안 됐을 경우도 대비
        if (ctrl.agent == null) return;

        ctrl.agent.SetDestination(ctrl.Target.position);

        float dist = Vector3.Distance(ctrl.transform.position, ctrl.Target.position);
        if (dist < ctrl.Data.attackRange)
            ctrl.ChangeState(new AttackState(ctrl));
        else if (!ctrl.CanSeePlayer())
            ctrl.ChangeState(new AlertState(ctrl));
    }
}