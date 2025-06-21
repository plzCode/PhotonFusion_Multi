using Fusion;
using UnityEngine;
using UnityEngine.AI;
using Zombie.States;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ZombieController))]
public class ZombieAIController : NetworkBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public NavMeshAgent agent;

    [Networked, HideInInspector] public NetworkObject TargetNetObj { get; private set; }
    public Transform Target => TargetNetObj ? TargetNetObj.transform : null;

    /* ─ Config 접근 ─ */
    public ZombieConfig Data => zCtrl.Data;

    /* ─ 내부 ─ */
    ZombieController zCtrl;
    ZombieState current;

    public bool IsAttacking { get; set; }

    const int REFRESH_TICKS = 30;     // 약 0.5초(60Hz)마다 갱신

    /* ========== 초기화 ========== */
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        zCtrl = GetComponent<ZombieController>();
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

    public override void Spawned()
    {

        if (HasStateAuthority)               // Host만 타깃 계산
            TargetNetObj = GetNearestPlayer();

        ChangeState(new IdleWalkState(this));
    }

    /* ========== 상태 머신 ========== */
    public void ChangeState(ZombieState next)
    {
        current?.Exit();
        current = next;
        current.Enter();

        Debug.Log($"{name} ▶ {next.GetType().Name}");
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && Runner.Tick % 60 == 0)
            Debug.Log($"{name} Target = {TargetNetObj?.name}");

        current?.Update();


        if (HasStateAuthority && Runner.Tick % REFRESH_TICKS == 0 || TargetNetObj == null)
            TargetNetObj = GetNearestPlayer();
    }

    public void SetMoveSpeed(float s) => agent.speed = s;

    public bool CanSeePlayer(float maxDist = 15f, float fov = 120f)
    {
        if (Target == null) return false;

        Vector3 dir = (Target.position - transform.position);
        if (dir.sqrMagnitude > maxDist * maxDist) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > fov * 0.5f) return false;

        // 레이캐스트로 가림 확인
        if (Physics.Raycast(transform.position + Vector3.up * 1.2f,
                            dir.normalized, out RaycastHit hit, maxDist,
                            LayerMask.GetMask("Default", "Environment", "Player")))
            return hit.transform == Target;

        return true;
    }
    public void PlayBlend(float speed01)
    {
        anim.SetFloat("Speed", speed01);
    }
    /* 소리 감지용 간단 콜백 (선택) */
    public void OnHearSound(Vector3 pos)
    {
        if (!HasStateAuthority) return;
        float dist = Vector3.Distance(transform.position, pos);
        if (dist < 20f)                     // 청각 반응 반경
            ChangeState(new AlertState(this));
    }

    public void HandleAttackHit()
    {
        // 1) 서버(Host)에서만 판정
        if (!HasStateAuthority || !IsAttacking) return;

        // 2) 타깃 플레이어 존재
        if (Target == null) return;

        /* 3) SphereCast 판정 */
        float radius = 0.45f;                   // 팔 두께
        float maxDist = Data.attackRange;        // SO 사거리
        Vector3 origin = transform.position + Vector3.up * 1.2f;

        if (Physics.SphereCast(origin, radius, transform.forward,
                               out RaycastHit hit, maxDist,
                               LayerMask.GetMask("Player")))
        {
            var pc = hit.collider.GetComponent<PlayerController>();
            if (pc != null)
                pc.TakeDamage(Data.damage);      // 4) 데미지 적용
        }
        IsAttacking = false; //중복 방지
    }
}
