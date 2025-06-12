using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieController : NetworkBehaviour
{
    [SerializeField] private ZombieConfig data;

    public ZombieConfig Data => data;

    /* 네트워크 동기화 변수 */
    [Networked] public int CurrentHP { get; set; }
    [Networked] public int ConfigId { get; set; }  // SO 식별 번호

    private NavMeshAgent agent;
    private Transform target;     // 플레이어 Transform

    /* 공격 파라미터 */
    float attackTimer;
    const float ATTACK_CD = 1.0f;          // 1초 쿨
    const float ATTACK_RANGE = 1.3f;

    public void Init(ZombieConfig cfg)           // ← 스폰 직전에 호출
    {
        data = cfg;
        CurrentHP = cfg.maxHP;
        ConfigId = ZombieConfigRegistry.GetId(cfg);
    }
    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = data.moveSpeed;

        target = GameObject.FindWithTag("Player")?.transform;

        if (HasStateAuthority)
            CurrentHP = data.maxHP;

        /* 타입 표시(디버그) */
        var tm = new GameObject("Label").AddComponent<TextMesh>();
        tm.text = data.specialType.ToString();
        tm.characterSize = 0.2f;
        tm.color = Color.yellow;
        tm.transform.SetParent(transform);
        tm.transform.localPosition = Vector3.up * 1.5f;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || target == null) return;

        /* 추적 */
        agent.SetDestination(target.position);

        /* 공격 쿨다운 */
        attackTimer -= Runner.DeltaTime;
        if (attackTimer <= 0 &&
            Vector3.Distance(transform.position, target.position) < ATTACK_RANGE)
        {
            /* 서버가 피해 계산 → 클라이언트(타깃) RPC */
            var hp = target.GetComponent<PlayerHealth>();
            hp?.TakeDamage(data.damage);        // 서버 로컬

            attackTimer = ATTACK_CD;

            /* 시각·사운드 RPC 예시 */
            RPC_PlayHit(target.GetComponent<NetworkObject>().InputAuthority);
        }
    }

    /* 클라이언트 타깃 전용 RPC – 화면 흔들림, 피격 사운드 등 */
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    void RPC_PlayHit(PlayerRef owner, RpcInfo info = default)
    {
        Debug.Log("[Player] Got hit!");
        // owner 변수를 쓰지 않으면 생략해도 됨
        /* TODO: UI HitFlash, 카메라 Shake 등 */
    }

    public void TakeDamage(int dmg)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Max(CurrentHP - dmg, 0);

        if (CurrentHP == 0)
            Runner.Despawn(Object);                // 네트워크 제거
    }

}
