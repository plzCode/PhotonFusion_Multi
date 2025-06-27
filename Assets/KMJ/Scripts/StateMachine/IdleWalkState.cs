using UnityEngine;
using Random = UnityEngine.Random;
using Zombie.States;

public class IdleWalkState : ZombieState
{
    /*──────────────── 설정 값 ────────────────*/
    const float IDLE_MIN = 1.5f;   //   1.5 ~ 3.0 초
    const float IDLE_MAX = 3.0f;
    const float WALK_MIN = 2.5f;   //   2.5 ~ 4.5 초
    const float WALK_MAX = 4.5f;
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

        /*── 플레이어 감지 시 즉시 Chase 로 전환 ──*/
        if (ctrl.InSightFov)
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