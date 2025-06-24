using Fusion;
using UnityEngine;
using UnityEngine.AI;
using Zombie.States;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ZombieController))]
public class ZombieAIController : NetworkBehaviour
{
    [Networked, HideInInspector] public NetworkObject TargetNetObj { get; private set; }
    public Transform Target => TargetNetObj ? TargetNetObj.transform : null;

    [SerializeField] float walkSpeed = 0.7f;
    [SerializeField] float runSpeed = 1.5f;

    //[SerializeField] float hearRadius = 15f;

    [SerializeField] public float walkRadius = 4f;

    [Networked] public PlayerRef LastAttacker { get; private set; }
    //float lastHitTime = -10f;

    [SerializeField] public float idleTimeMax = 3; // Idle 상태 최대 시간 (초 단위, 0 = 무제한 대기)

    public bool IsAlert { get; set; }               // 애니메이터 Bool 연동
    public bool IsAttacking { get; set; }

    /* ─ Config 접근 ─ */
    public ZombieConfig Data => zCtrl.Data;

    /* ─ 내부 ─ */
    [HideInInspector] public Animator anim;
    [HideInInspector] public NavMeshAgent agent;
    ZombieController zCtrl;
    ZombieState current;

    const int REFRESH_TICKS = 30;     // 약 0.5초(60Hz)마다 갱신
    const float DETECT_RADIUS = 6F;

    /* ========== 초기화 ========== */
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        zCtrl = GetComponent<ZombieController>();
    }

    public override void Spawned()
    {

        if (HasStateAuthority)               // Host만 타깃 계산
            TargetNetObj = FindNearestPlayer();

        ChangeState(new IdleState(this));
    }

    NetworkObject FindNearestPlayer()
    {
        NetworkObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var player in GameManager.Players)
        {
            if (player == null) continue;                   // 누락/파괴 대비
            float d = (player.transform.position - transform.position).sqrMagnitude;
            if (d < minDist)
            {
                minDist = d;
                nearest = player;
            }
        }
        return nearest;

    }

    public override void FixedUpdateNetwork()
    {

        if (!agent.isOnNavMesh) // NavMesh 안전 장치
        {
            // 가장 가까운 NavMesh 표면에 붙여 줌
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);

            return;                     // 이번 Tick 로직 건너뜀
        }

        current?.Update();

        if (HasStateAuthority && Runner.Tick % REFRESH_TICKS == 0)
        {
            TargetNetObj = FindNearestPlayer();

            // ◀ 시야 or 소리 감지
            if (TargetNetObj && DetectPlayerNearby())
                ChangeState(new AlertState(this));
        }
    }

    /* ========== 상태 머신 ========== */
    public void ChangeState(ZombieState next)
    {
        current?.Exit();
        current = next;
        current.Enter();

        Debug.Log($"{name} ▶ {next.GetType().Name}");
    }

    public Vector3 RandomPatrolPoint(float radius)
    {
        for (int i = 0; i < 8; i++)                      // 최대 8회 시도
        {
            Vector3 rnd = transform.position +
                          Random.insideUnitSphere * radius;
            rnd.y = transform.position.y;

            if (UnityEngine.AI.NavMesh.SamplePosition(rnd, out var hit, 1f, NavMesh.AllAreas))
                return hit.position;                     // NavMesh 위 좌표 반환
        }
        return transform.position;                       // 실패 시 현재 위치
    }

    public void SetMoveSpeed(float v)
    {
        anim.SetFloat("Speed", v);

        if (!agent.isOnNavMesh) return;

        if (v < 0.05f)        // Idle
        {
            agent.isStopped = true;
            agent.speed = 0f;
        }
        else
        {
            agent.isStopped = false;
            agent.speed = Mathf.Lerp(walkSpeed, runSpeed, v);  // Walk/Run
        }
    }
    public void PlayTrigger(string trig) => anim.SetTrigger(trig);

    // ───────── sensing ─────────
    public bool DetectPlayerNearby()
    {
        if (Target == null) return false;

        // 거리² ≤ 반경²  →  감지 성공
        return (Target.position - transform.position).sqrMagnitude
               <= DETECT_RADIUS * DETECT_RADIUS;
    }


    public void SetAlert(bool on) => IsAlert = on;

    public bool IsInAttackRange
    {
        get
        {
            if (Target == null) return false;
            if (agent.pathPending) return false;          // 아직 경로 계산 중
            return agent.remainingDistance <= Data.attackRange + 0.05f;
        }
    }

    //public void OnHearSound(Vector3 soundPos)
    //{
    //    // 소리가 hearRadius 안이면 즉시 Alert 상태로
    //    if ((soundPos - transform.position).sqrMagnitude <= hearRadius * hearRadius)
    //        ChangeState(new AlertState(this));
    //}

    public void HandleAttackHit()
    {
        // 1) 서버(Host)에서만 판정
        if (!HasStateAuthority || !IsAttacking) return;

        // 2) 타깃 플레이어 존재
        if (Target == null) return;

        Vector3 origin = transform.position + Vector3.up * 1.2f;
        if (Physics.SphereCast(origin, 0.45f, transform.forward,
                               out var hit, Data.attackRange,
                               LayerMask.GetMask("Player")))
        {
            
            var pc = hit.collider.GetComponentInParent<PlayerController>();
            if (pc) pc.TakeDamage(Data.damage);    // 4) 데미지 적용
        }
        IsAttacking = false; //중복 방지
    }
}
