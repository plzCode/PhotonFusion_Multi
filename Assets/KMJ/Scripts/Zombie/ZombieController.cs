using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(SpecialZombieController))]
[RequireComponent(typeof(ZombieAIController))]

public class ZombieController : NetworkBehaviour
{
    [SerializeField] ZombieConfig data;

    /* 네트워크 동기화 변수 */
    [Networked] public int CurrentHP { get; set; }
    [Networked] public int ConfigId { get; set; }

    /* ───── 내부 ───── */
    NavMeshAgent agent;
    ZombieAIController ai;
    Animator anim;

    /* 공격 파라미터 */
    float attackTimer;
    const float ATTACK_CD = 1.0f;

    public ZombieConfig Data => data;          // 외부 참조용 getter

    /*========== 초기화(웨이브 스폰 시) ==========*/
    public void Init(ZombieConfig cfg)
    {
        data = cfg;
        CurrentHP = cfg.maxHP;
        ConfigId = ZombieConfigRegistry.GetId(cfg);

        GetComponent<SpecialZombieController>().Init(cfg); //특수 능력
    }
    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        ai = GetComponent<ZombieAIController>();
        anim = GetComponentInChildren<Animator>();

        agent.speed = data.moveSpeed;
        attackTimer = 0f;

        if (HasStateAuthority)
            CurrentHP = data.maxHP;
    }

    /*========== 메인 루프 ==========*/
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        /* 1) 이동 속도 → 애니메이션 파라미터 (없으면 무시) */
        anim?.SetFloat("Speed", agent.velocity.magnitude);

        /* 2) 공격 판정 */
        attackTimer -= Runner.DeltaTime;

        if (ai.CurrentState == ZombieAIController.State.Chase &&
            ai.Target != null &&
            Vector3.Distance(transform.position, ai.Target.position) < data.attackRange)
        {
            if (attackTimer <= 0f)
            {
                attackTimer = ATTACK_CD;
                anim?.SetTrigger("Attack");

                /* RPC 로 피해 전송 */
                RPC_DealDamage(ai.Target.GetComponent<NetworkObject>().InputAuthority,
                               data.damage);
            }
        }
    }

    /*========== HP 감소 / 사망 ==========*/
    public void TakeDamage(int dmg)
    {
        if (!HasStateAuthority) return;

        CurrentHP = Mathf.Max(CurrentHP - dmg, 0);
        if (CurrentHP == 0) Die();
    }
    void Die()
    {
        anim?.SetBool("IsDead", true);                       // 죽음 애니 (옵션)
        GetComponent<SpecialZombieController>().OnDeath();   // 특수 효과 발동
        Runner.Despawn(Object);                              // 네트워크 제거
    }

    /*========== RPC: 플레이어에게 데미지 ==========*/
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    void RPC_DealDamage(PlayerRef recv, int dmg, RpcInfo info = default)
    {
        var hp = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();

        hp?.TakeDamage(dmg);
    }

}
