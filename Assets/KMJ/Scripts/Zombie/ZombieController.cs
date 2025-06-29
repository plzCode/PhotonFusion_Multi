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


    [Header("Vision")]
    [SerializeField] Transform eyePoint;      // ← 씬/프리팹에서 ‘눈 위치’ 트랜스폼 드래그
    [SerializeField] LayerMask obstacleMask;  // ← “Default, Environment, Player” 등 가림 레이어

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
    public bool CanSeePlayer(Transform target, float maxDist = 15f, float fov = 120f)
    {
        if (!target) return false;

        Vector3 dir = target.position - eyePoint.position;
        if (dir.sqrMagnitude > maxDist * maxDist) return false;

        if (Vector3.Angle(eyePoint.forward, dir) > fov * 0.5f) return false;

        if (Physics.Raycast(eyePoint.position, dir.normalized, out var hit,
                            maxDist, obstacleMask, QueryTriggerInteraction.Ignore))
            return hit.transform.root == target.root;   // 팔/총 무시

        return true;
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
        ai.ChangeState(new HitState(ai));      // 짧은 경직
    }

    /*========== 사망 처리 ==========*/
    void Die()
    {
        ai.ChangeState(new DieState(ai));
        StartCoroutine(WaitAndDespawn());
    }

    IEnumerator WaitAndDespawn()
    {
        yield return new WaitForSeconds(2f);           // Die 애니 2초 감상
        if (Object && Object.HasStateAuthority)
            Runner.Despawn(Object);
    }


}
