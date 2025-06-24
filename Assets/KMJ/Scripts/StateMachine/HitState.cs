using UnityEngine;
using Zombie.States;


public class HitState : ZombieState
{

    public HitState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.SetMoveSpeed(0f);
        ctrl.anim.SetTrigger("Hit");
    }

    public override void Update()
    {

        // 0.6초 뒤 Chase 재개
        if (ctrl.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            ctrl.ChangeState(new RunState(ctrl));
    }
}
