using UnityEngine;
using Zombie.States;

public class AlertState : ZombieState
{
    const float ALERT_TIME = 2f;  // 실제 체감 시간
    const float CLIP_LENGTH = 7f;  // 원본 클립 길이
    float timer;

    public AlertState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        if (!ctrl.agent.enabled || !ctrl.agent.isOnNavMesh) return;

        // 1) 이동 완전 정지
        ctrl.agent.isStopped = true;
        ctrl.agent.speed = 0f;
        ctrl.anim.SetFloat("Speed", 0f);

        // 2) 경계 애니메이션 배속 조정 (7s → 3s)
        ctrl.anim.speed = 3f;
        ctrl.anim.SetBool("IsAlert", true);           // Animator 전이
        ctrl.anim.CrossFade("Alert", 0.05f);

        timer = 2f;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ctrl.agent.isStopped = false;
            ctrl.anim.SetBool("IsAlert", false);
            ctrl.anim.SetFloat("Speed", 1f);
            ctrl.ChangeState(new ChaseState(ctrl));
        }
    }

    public override void Exit()
    {
        ctrl.anim.SetBool("IsAlert", false);
        ctrl.anim.speed = 1f;
        ctrl.agent.isStopped = false;
    }
}
