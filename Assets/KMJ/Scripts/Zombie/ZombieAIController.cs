using Fusion;
using UnityEngine;
using UnityEngine.AI;
using Zombie.States;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ZombieController))]
public class ZombieAIController : NetworkBehaviour
{
    /* ────────── 캐시 ────────── */
    public Animator anim { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public ZombieController zCtrl;
    int hitCount = 0;

    /* ────────── 설정 ────────── */
    [Header("Sense")] public float alertRadius = 8f;
    [Range(30f, 180f)] public float fovDeg = 120f;
    [Header("Speed")] 
    public float walkSpeed = 1.2f;
    public float runSpeed = 4.5f;

    public Vector3 pendingGoal { get; private set; } // 웨이브 스폰 시 이동 목표

    /* ────────── Networked 상태 ────────── */
    [Networked] public NetworkObject TargetNetObj { get; private set; }
    public Transform Target => TargetNetObj ? TargetNetObj.transform : null;

    [Networked] public bool InAlertRadius { get; private set; }
    [Networked] public bool InSightFov { get; private set; }
    [Networked] public bool InAttackRange { get; private set; }
    [Networked] public bool IsAttacking { get; set; }

    /* ────────── 로컬 상태 ────────── */
    ZombieState current;
    const int RESELECT_TICKS = 60;   // 1 s

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
            TargetNetObj = null;

        ChangeState(new IdleWalkState(this));
    }
    /* ========== 상태 머신 ========== */
    public void ChangeState(ZombieState next)
    {
        current?.Exit();
        current = next;
        current.Enter();
        //Debug.Log($"{name} ▶ {next.GetType().Name}");
    }

    // ---------- 센싱 ----------
    public void SensePlayer()
    {
        /* ❶ 타깃이 null 이거나 파괴된 경우 재탐색 */
        if (Target == null)
        {
            InAlertRadius = InSightFov = InAttackRange = false;
            return;
        }

        Vector3 toT = Target.position - transform.position;
        float sqr = toT.sqrMagnitude;

        /* 반경 */
        InAlertRadius = sqr < alertRadius * alertRadius;

        /* 시야 & 가림 */
        float angle = Vector3.Angle(transform.forward, toT);
        InSightFov = angle < fovDeg * 0.5f &&
                     zCtrl.CanSeePlayer(Target, alertRadius, fovDeg);

        /* 근접 */
        float atkR = zCtrl.Data ? zCtrl.Data.attackRange : 1f;
        InAttackRange = sqr < atkR * atkR;

    }

    // ---------- 메인 루프 ----------
    public override void FixedUpdateNetwork()
    {
        /* 서버에서만 타깃 재설정 */
        if (HasStateAuthority && Runner.Tick % RESELECT_TICKS == 0)
            RetargetIfNeeded();

        if (HasStateAuthority) SensePlayer();

        current?.Update();
        anim.SetBool("InAttackRange", InAttackRange);
    }

    void RetargetIfNeeded()
    {
        /* ❶ 기존 타깃이 null/파괴되면 비우기 */
        if (TargetNetObj == null || !TargetNetObj || !TargetNetObj.IsValid)
        {
            TargetNetObj = null;
        }
        else
        {
            var pc = TargetNetObj.GetComponent<PlayerController>();
            if(pc != null && (!pc.isAlive || pc.isClear))
            {
                TargetNetObj = null;
            }
        }

        /* ❷ 아직 타깃 없고, 근처에 감지된 플레이어가 있으면 지정 */
        if (TargetNetObj == null)
        {
            var cand = GetNearestPlayerWithinRadius(alertRadius);
            if (cand) TargetNetObj = cand;
        }
    }

    public void SpawnAggro(Vector3 epicenter)
    {
        pendingGoal = epicenter;              
        TargetNetObj = null;                      
        ChangeState(new ChaseState(this));
    }

    NetworkObject GetNearestPlayerWithinRadius(float radius)
    {
        NetworkObject nearest = null;
        float minDist = radius * radius; // 제곱 거리로 비교

        foreach (var player in GameManager.Players)
        {
            if (player == null || !player.GetComponent<PlayerController>().isAlive || player.GetComponent<PlayerController>().isClear) continue;                   // 누락/파괴 대비
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
        Debug.Log($"[HandleAttackHit] 호출 #{++hitCount}");

        if (!HasStateAuthority || !IsAttacking)
        {
            return;
        }

        if (Target == null)
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 1.2f;
        float range = zCtrl.Data?.attackRange ?? 1f;

        if (Physics.SphereCast(origin, 0.45f, transform.forward,
                               out RaycastHit hit, range,
                               LayerMask.GetMask("Player")))
        {
            var pc = hit.collider.GetComponentInParent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(zCtrl.Data.damage);
            }

        }

        IsAttacking = false;  // 중복 방지
    }

}
