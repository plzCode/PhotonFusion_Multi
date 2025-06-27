using Fusion;
using UnityEngine;
using UnityEngine.AI;
using Zombie.States;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ZombieController))]
public class ZombieAIController : NetworkBehaviour
{
    [Header("Refs")]
    public Animator anim { get; private set; }
    public NavMeshAgent agent { get; private set; }
    ZombieController zCtrl;

    [Header("Sense Settings")]
    [SerializeField] float alertRadius = 8f;   // 반경 감지 (m)
    [SerializeField] float fovDeg = 120f;  // 시야각 (°)

    [Header("Move Speeds")]
    public float walkSpeed = 1.2f;
    public float runSpeed = 4.5f;    // 추적 속도

    public Vector3 pendingGoal { get; private set; }

    /* ───── Network-Synced 플래그 ───── */
    [Networked] public bool InAlertRadius { get; private set; }
    [Networked] public bool InSightFov { get; private set; }
    [Networked] public bool InAttackRange { get; private set; }
    [Networked] public bool IsAttacking { get; set; }

    /* ───── 기타 ───── */
    [Networked] public NetworkObject TargetNetObj { get; private set; }
    public Transform Target => TargetNetObj ? TargetNetObj.transform : null;



    ZombieState current;
    const int REFRESH_TICKS = 30;   // 0.5 s
    public ZombieState CurrentState => current;

    /* ========== 초기화 ========== */
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        zCtrl = GetComponent<ZombieController>();
    }

    public void ChangeState(ZombieState next)
    {
        if (current != null) current.Exit();
        current = next;
        if (current != null) current.Enter();
    }


    public override void Spawned()
    {
        if (HasStateAuthority)               // Host만 타깃 계산
            TargetNetObj = GetNearestPlayer();

        ChangeState(new IdleWalkState(this));
    }

    /* ====================== 메인 루프 ====================== */
    public override void FixedUpdateNetwork()
    {

        /* 1) 타깃 재탐색 (서버) */
        if (HasStateAuthority &&
            (TargetNetObj == null || Runner.Tick % REFRESH_TICKS == 0))
            TargetNetObj = GetNearestPlayer();

        /* 2) 센서 플래그 갱신 (서버) */
        if (HasStateAuthority)
            SensePlayer();

        if (!(current is AlertState) && Target && agent.enabled && agent.isOnNavMesh)
            agent.SetDestination(Target.position);

        /* 3) 상태 머신 */
        current?.Update();

        /* 4) 애니메이터 파라미터 공통 처리 */
        anim.SetBool("InAttackRange", InAttackRange);

    }

    void SetTarget(NetworkObject netObj)
    {
        if (!HasStateAuthority) return;        // 서버/호스트에서만
        TargetNetObj = netObj;                 // ← Networked<PlayerRef> 변수
    }

    /* ====================== 센싱 ====================== */
    public void SensePlayer()
    {
        if (!Target)
        {
            InAlertRadius = InSightFov = InAttackRange = false;
            return;
        }

        if (!InSightFov) InAlertRadius = false;

        Vector3 toT = Target.position - transform.position;
        float sqr = toT.sqrMagnitude;
        /* 반경 */
        InAlertRadius = sqr < alertRadius * alertRadius;
        /* 시야 + 가림 */
        float angle = Vector3.Angle(transform.forward, toT);
        InSightFov = angle < fovDeg * 0.5f && zCtrl.CanSeePlayer(Target, alertRadius, fovDeg);

        /* 근접 사거리 – ZombieConfig 값 사용 */
        float atkR = zCtrl.Data ? zCtrl.Data.attackRange : 1f;
        InAttackRange = sqr < atkR * atkR;
    }

    /* ========== 상태 머신 ========== */
    public void SpawnAggro(Vector3 epicenter)
    {
        pendingGoal = epicenter;
        SetTarget(GetNearestPlayer());
        agent.speed = runSpeed;
        agent.isStopped = false;
        anim.SetFloat("Speed", 1f);
        ChangeState(new ChaseState(this));
    }

    NetworkObject GetNearestPlayer()
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

    NetworkObject GetNearestPlayerWithinRadius(float radius)
    {
        NetworkObject nearest = null;
        float minDist = radius * radius; // 제곱 거리로 비교

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

    public void HandleAttackHit()
    {
        // 1) 서버(Host)에서만 판정
        if (!HasStateAuthority || !IsAttacking) return;

        // 2) 타깃 플레이어 존재
        if (Target == null) return;

        /* 3) SphereCast 판정 */
        float radius = 0.45f;                   // 팔 두께
        float maxDist = zCtrl.Data.attackRange;        // SO 사거리
        Vector3 origin = transform.position + Vector3.up * 1.2f;

        if (Physics.SphereCast(origin, radius, transform.forward,
                               out RaycastHit hit, maxDist,
                               LayerMask.GetMask("Player")))
        {
            var pc = hit.collider.GetComponent<PlayerController>();
            if (pc != null)
                pc.TakeDamage(zCtrl.Data.damage);      // 4) 데미지 적용
        }
        IsAttacking = false; //중복 방지
    }
}
