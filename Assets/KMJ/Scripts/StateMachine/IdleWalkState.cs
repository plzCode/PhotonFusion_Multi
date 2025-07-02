using UnityEngine;
using Random = UnityEngine.Random;
using Zombie.States;

public class IdleWalkState : ZombieState
{
    /*──────────────── 설정 값 ────────────────*/
    const float IDLE_MIN = 3f, IDLE_MAX = 5f;
    const float WALK_MIN = 3f, WALK_MAX = 5f;
    const float WALK_SPEED = 0.5f;   // Animator “Speed” 값 (0 = Idle, 1 = Run)

    readonly float idleRadius = 3f;  // 현 위치 기준 걷기 반경(m)

    /*──────────────── 상태 변수 ───────────────*/
    float timer;
    bool isWalking;

    public IdleWalkState(ZombieAIController ai) : base(ai) { }

    /*────────────── 진입 & 갱신 & 종료 ─────────────*/
    public override void Enter()
    {
        SwitchToIdle();                    // 최초 = Idle
        ctrl.zCtrl.SfxIdleWalk();
    }

    public override void Exit()
    {
        ctrl.agent.isStopped = true;         // 다음 상태로 넘어갈 때 정지
        ctrl.anim.SetFloat("Speed", 0f);
    }

    public override void Update()
    {
        ctrl.SensePlayer();

        timer -= Time.deltaTime;

        // 타이머 만료 → Idle ↔ Walk 토글
        if (timer <= 0f)
        {
            if (isWalking) SwitchToIdle();
            else SwitchToWalk();
        }

        if (ctrl.InAlertRadius || ctrl.InSightFov)
        {
            ctrl.ChangeState(new AlertState(ctrl));
            return;
        }
    }

    /*──────────────── 헬퍼 메서드 ───────────────*/
    void SwitchToIdle()
    {
        isWalking = false;
        timer = Random.Range(IDLE_MIN, IDLE_MAX);
        if (ctrl.agent.enabled)
            ctrl.agent.isStopped = true;
        ctrl.anim.SetFloat("Speed", 0f);     // BlendTree → Idle
    }

    void SwitchToWalk()
    {
        isWalking = true;
        timer = Random.Range(WALK_MIN, WALK_MAX);

        // 반경 안 랜덤 지점 선택
        Vector2 off = Random.insideUnitCircle.normalized * idleRadius;
        Vector3 dst = ctrl.transform.position + new Vector3(off.x, 0f, off.y);

        ctrl.agent.SetDestination(dst);
        ctrl.agent.isStopped = false;
        ctrl.anim.SetFloat("Speed", WALK_SPEED); // 0.5 → Walk
    }
}