using UnityEngine;
using Zombie.States;

class IdleState : ZombieState
{
    float wait;

    public IdleState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.SetMoveSpeed(0f);
        wait = Random.Range(1f, ctrl.idleTimeMax);   // 1~3초 휴식
    }

    public override void Update()
    {
        // 플레이어를 보면 즉시 Alert
        if (ctrl.DetectPlayerNearby())
        {
            ctrl.ChangeState(new AlertState(ctrl));
            return;
        }
        wait -= Time.deltaTime;
        if (wait <= 0f)
            ctrl.ChangeState(new WalkState(ctrl));
    }
}
