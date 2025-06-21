using UnityEngine;
using Zombie.States;

public class AttackState : ZombieState
{

    public AttackState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.IsAttacking = true;
        ctrl.SetMoveSpeed(0f);
        ctrl.anim.ResetTrigger("Hit");             // 혹시 남아 있던 트리거 클리어
        ctrl.anim.SetTrigger("Attack");            // 공격 애니 트리거
    }

    public override void Update()
    {
        // 애니메이션이 끝났는지 검사 (layer 0, Attack 태그)
        if (ctrl.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") == false)
        {
            ctrl.IsAttacking = false;
            ctrl.ChangeState(new ChaseState(ctrl));
        }
    }
}
