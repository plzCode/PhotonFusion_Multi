using UnityEngine;
using Zombie.States;

public class AttackState : ZombieState
{
    bool damageDone;

    public AttackState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        damageDone = false;
        ctrl.SetMoveSpeed(0f);
        ctrl.anim.ResetTrigger("Hit");             // 혹시 남아 있던 트리거 클리어
        ctrl.anim.SetTrigger("Attack");            // 공격 애니 트리거
    }

    // Animation Event 로 호출 (클립에서 OnAttackHit())
    public void OnAttackHitEvent()
    {
        if (damageDone) return;               // 중복 방지
        damageDone = true;

        if (!ctrl.HasStateAuthority) return;

        // 데미지 판정
        if (ctrl.Target != null &&
            Vector3.Distance(ctrl.transform.position,
                             ctrl.Target.position) < ctrl.Data.attackRange + 0.2f)
        {
            var hp = ctrl.Target.GetComponent<PlayerHealth>();
            if (hp) hp.RPC_TakeDamage(ctrl.Data.damage);
        }
    }

    public override void Update()
    {
        // 애니메이션이 끝났는지 검사 (layer 0, Attack 태그)
        if (ctrl.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") == false)
        {
            ctrl.ChangeState(new ChaseState(ctrl));
        }
    }
}
