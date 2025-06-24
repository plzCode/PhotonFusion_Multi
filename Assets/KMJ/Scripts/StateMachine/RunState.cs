using UnityEngine;
using UnityEngine.AI;
using Zombie.States;

class RunState : ZombieState
{
    public RunState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.agent.stoppingDistance = ctrl.Data.attackRange;
        ctrl.SetMoveSpeed(1f);
    }

    public override void Update()
    {
        if (ctrl.Target)
            ctrl.agent.SetDestination(ctrl.Target.position);

        /* 사거리 진입 → Attack */
        if (ctrl.IsInAttackRange)
        {
            ctrl.ChangeState(new AttackState(ctrl));
            return;
        }

        /* 플레이어를 잃었으면 Idle */
        if (!ctrl.DetectPlayerNearby())
            ctrl.ChangeState(new IdleState(ctrl));
    }
}
