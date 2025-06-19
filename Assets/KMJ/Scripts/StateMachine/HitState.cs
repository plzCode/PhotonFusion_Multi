using UnityEngine;
using Zombie.States;


public class HitState : ZombieState
{
    float timer;

    public HitState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        timer = 0f;
        ctrl.SetMoveSpeed(0f);
        ctrl.anim.SetTrigger("Hit");          // Hit 애니 트리거
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        // 0.6초 뒤 Chase 재개
        if (timer >= 0.6f)
            ctrl.ChangeState(new ChaseState(ctrl));
    }
}
