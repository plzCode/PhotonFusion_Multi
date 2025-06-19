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

    /* ========== 초기화 ========== */
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        zCtrl = GetComponent<ZombieController>();
    }


    public override void Spawned()
    {
        /* Host가 플레이어 지정 (임시: 첫 번째 Player 태그) */
        if (HasStateAuthority && TargetNetObj == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO) TargetNetObj = playerGO.GetComponent<NetworkObject>();
        }

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
        if (current != null)
            current.Update();
    }
    public void SetMoveSpeed(float s) => agent.speed = s;
    public bool CanSeePlayer(float maxDist = 15f, float fov = 120f)
    {
        if (Target == null) return false;

        Vector3 dir = (Target.position - transform.position);
        if (dir.sqrMagnitude > maxDist * maxDist) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > fov * 0.5f) return false;

        /* 레이캐스트 시야 가림 체크는 필요 시 추가 */
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
        if (current is DieState) return;

        ChangeState(new AlertState(this));
    }
}
