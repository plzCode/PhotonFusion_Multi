using UnityEngine;
using UnityEngine.AI;
using Zombie.States;

class WalkState : ZombieState
{
    int retry;
    float walkTime;

    public WalkState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.SetMoveSpeed(0.3f);
        ctrl.agent.SetDestination(ctrl.RandomPatrolPoint(ctrl.walkRadius));
        walkTime = Random.Range(2f, 4f);
        retry = 0;
    }

    public override void Update()
    {
        if (ctrl.DetectPlayerNearby())
        {
            ctrl.ChangeState(new AlertState(ctrl));
            return;
        }

        /* 목적지 도달 판정 */
        if (!ctrl.agent.pathPending && ctrl.agent.remainingDistance < 0.4f)
        {
            if (++retry > 2)                                    // 3회 실패
                ctrl.ChangeState(new IdleState(ctrl));          // 걷기 종료
            else
                ctrl.agent.SetDestination(ctrl.RandomPatrolPoint(4f));
        }

        walkTime -= Time.deltaTime;
        if (walkTime <= 0f)
            ctrl.ChangeState(new IdleState(ctrl));
    }
}
