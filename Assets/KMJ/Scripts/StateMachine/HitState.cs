using UnityEngine;
using Zombie.States;


public class HitState : ZombieState
{
    const float HIT_TIME = 0.6f;
    float t;

    public HitState(ZombieAIController c) : base(c)
    {
       
    }

    public override void Enter()
    {
        t = 0f;
        ctrl.anim.CrossFade("Hit", 0.05f);

        // 즉시 멈춤
        if (ctrl.agent.enabled && ctrl.agent.isOnNavMesh)
        {
            ctrl.agent.isStopped = true;
            ctrl.agent.velocity = Vector3.zero;
        }
    }

    public override void Update()
    {
        t += Time.deltaTime;
        if (t < HIT_TIME) return;

        bool shouldChase =
            ctrl.InAttackRange || ctrl.InSightFov || ctrl.InAlertRadius;

        if (shouldChase)
        {   // ▶ 추적 상태로 복귀
            ctrl.anim.SetFloat("Speed", 1f);           // Run 블렌드
            if (ctrl.agent.enabled && ctrl.agent.isOnNavMesh)
            {
                ctrl.agent.isStopped = false;
                ctrl.agent.speed = ctrl.runSpeed;
            }
            ctrl.ChangeState(new ChaseState(ctrl));
        }
        else
        {   // ▶ 순찰(Idle/Walk)로 복귀
            ctrl.anim.SetFloat("Speed", 0.15f);        // Walk 블렌드
            if (ctrl.agent.enabled && ctrl.agent.isOnNavMesh)
            {
                ctrl.agent.isStopped = false;
                ctrl.agent.speed = ctrl.walkSpeed;
            }
            ctrl.ChangeState(new IdleWalkState(ctrl));
        }
    }

    /* ---------- Exit ---------- */
    public override void Exit() { /* 특수 처리 없음 */ }
}