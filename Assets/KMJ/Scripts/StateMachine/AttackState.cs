using UnityEngine;
using Zombie.States;

public class AttackState : ZombieState
{
    float clipLen;            // 애니메이션 길이
    float t;                  // 경과 시간

    public AttackState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.IsAttacking = true;              // 중복 방지 플래그
        ctrl.SetMoveSpeed(0f);
        ctrl.PlayTrigger("Attack");           // Animator Trigger
        clipLen = ctrl.anim.GetCurrentAnimatorStateInfo(0).length;
        t = 0f;
    }

    public override void Update()
    {
        t += Time.deltaTime;   // ← Runner 접근 수정

        if (t >= clipLen)
        {
            ctrl.IsAttacking = false;
            ctrl.ChangeState(new RunState(ctrl));  // 공격 후 Idle
        }
    }
}
