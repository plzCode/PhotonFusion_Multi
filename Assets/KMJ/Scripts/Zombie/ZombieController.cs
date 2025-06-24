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

        if (ai.Target != null && Vector3.Distance(transform.position, ai.Target.position) < data.attackRange)
        {
            if (attackTimer <= 0f)
            {
                attackTimer = ATTACK_CD;
                anim?.SetTrigger("Attack");

                /* RPC 로 플레이어 피해 전송 */
                var pNet = ai.Target.GetComponent<NetworkObject>();
                if (pNet != null)
                    RPC_DealDamage(pNet.InputAuthority, data.damage);
            }
        }
    }

    /*========== 플레이어가 때릴 때 호출할 메서드 ==========*/
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int dmg, RpcInfo info = default)
    {
        if (CurrentHP <= 0) return;

        CurrentHP = Mathf.Max(CurrentHP - dmg, 0);
        if (CurrentHP > 0)
        {
            anim?.SetTrigger("Hit");                 // Hit 애니
            ai.ChangeState(new HitState(ai));        // 짧은 경직
        }
        else
        {
            Die();
        }
    }
    /*========== 사망 처리 ==========*/
    void Die()
    {
        anim?.SetBool("IsDead", true);                       // 죽음 애니 (옵션)
        GetComponent<SpecialZombieController>()?.OnDeath();   // 특수 효과 발동
        Runner.Despawn(Object);                              // 네트워크 제거
    }

    /*========== 좀비 → 플레이어 데미지 ==========*/
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    void RPC_DealDamage(PlayerRef recv, int dmg, RpcInfo info = default)
    {
        var hp = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        if (hp && Object.HasStateAuthority)          // Host에서만 판정
            hp.RPC_TakeDamage(dmg);                  // ← 시그니처 동일
    }

}
