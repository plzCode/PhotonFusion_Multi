using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ZombieAIController))]

public class ZombieController : NetworkBehaviour
{
    [SerializeField] ZombieConfig data;
    public ZombieConfig Data => data;          // 외부 참조용 getter

    /* 네트워크 동기화 변수 */
    [Networked] public int CurrentHP { get; set; }

    /* ───── 내부 ───── */
    NavMeshAgent agent;
    ZombieAIController ai;
    Animator anim;

    /*========== 초기화(웨이브 스폰 시) ==========*/
    public void Init(ZombieConfig cfg)
    {
        data = cfg;
        CurrentHP = cfg.maxHP;
    }


    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        ai = GetComponent<ZombieAIController>();
        anim = GetComponentInChildren<Animator>();

        if (HasStateAuthority && CurrentHP == 0)
            CurrentHP = data.maxHP;
    }

    /*========== 플레이어가 때릴 때 호출할 메서드 ==========*/
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RequestDamage(int dmg, RpcInfo info = default)
    {
        if (CurrentHP <= 0) return;

        if (!HasStateAuthority) return;
            CurrentHP = Mathf.Max(CurrentHP - dmg, 0);

        Debug.Log($"{name} HP {CurrentHP}");

        if (CurrentHP > 0)
        {
            RPC_Hit();
        }
        else
        {
            Die();                                 // 사망 처리
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Hit()
    {
        anim.SetTrigger("Hit");                // Hit 애니
        ai.ChangeState(new HitState(ai));      // 짧은 경직
    }

    /*========== 사망 처리 ==========*/
    void Die()
    {
        anim?.SetBool("IsDead", true);                       // 죽음 애니 (옵션)
        var special = GetComponent<SpecialZombieController>();
        if (special) special.OnDeath();   // 특수 효과 발동
        agent.enabled = false;                               // 이동정지

        StartCoroutine(WaitAndDespawn());
    }

    IEnumerator WaitAndDespawn()
    {
        yield return new WaitForSeconds(2f);           // Die 애니 2초 감상
        if (Object && Object.HasStateAuthority)
            Runner.Despawn(Object);
    }
}
