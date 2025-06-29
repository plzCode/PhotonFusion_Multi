using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ZombieAIController))]

public class ZombieController : NetworkBehaviour
{
    [SerializeField] ZombieConfig data;
    public ZombieConfig Data => data;          // 외부 참조용 getter

    /* 네트워크 동기화 변수 */
    [Networked] public int CurrentHP { get; set; }

    /* ─────────── Audio Clips (Inspector) ─────────── */
    [Header("SFX")]
    public AudioClip idleWalkLoop;     // Idle & Walk 숨소리 (Loop)
    public AudioClip chaseLoop;        // Chase 숨소리  (Loop)
    public AudioClip alertClip;        // Alert 포효  (One-shot)
    public AudioClip attackClip;       // Swing
    public AudioClip hitClip;          // 피격
    public AudioClip deathClip;        // 사망

    AudioSource amb;   // 루프 전용
    AudioSource sfx;   // One-shot

    [Header("Vision")]
    [SerializeField] Transform eyePoint;      // ← 씬/프리팹에서 ‘눈 위치’ 트랜스폼 드래그
    [SerializeField] LayerMask obstacleMask;  // ← “Default, Environment, Player” 등 가림 레이어

    /* ───── 내부 ───── */
    NavMeshAgent agent;
    ZombieAIController ai;
    Animator anim;

    /*========== 초기화(웨이브 스폰 시) ==========*/

    void Awake()
    {
        _SetupAudio();
    }

    public void Init(ZombieConfig cfg)
    {
        data = cfg;
        CurrentHP = cfg.maxHP;
        PlayIdleLoop();
    }
    void _SetupAudio()
    {
        amb = gameObject.AddComponent<AudioSource>();
        amb.loop = true; amb.spatialBlend = 1; amb.minDistance = 2; amb.maxDistance = 20;

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.loop = false; sfx.spatialBlend = 1; sfx.minDistance = 2; sfx.maxDistance = 25;
    }
    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        ai = GetComponent<ZombieAIController>();
        anim = GetComponentInChildren<Animator>();

        if (HasStateAuthority && CurrentHP == 0)
            CurrentHP = data.maxHP;

        var smr = GetComponentInChildren<SkinnedMeshRenderer>();
        if (!smr) return;

        Material[] mats = smr.materials;      // 자동 복제
        Color body = new Color(0.7f, 0.85f, 0.8f);   // 청록빛 피부
        Color cloths = new Color(0.25f, 0.2f, 0.15f);    // 거무스름 옷

        if (mats.Length > 0) mats[0].SetColor("_BaseColor", body);
        if (mats.Length > 1) mats[1].SetColor("_BaseColor", cloths);

        smr.materials = mats;
    }

    public void SfxIdleWalk() => PlayIdleLoop();
    public void SfxAlert() { PlayOneShot(alertClip); PlayChaseLoop(); }
    public void SfxChase() => PlayChaseLoop();
    public void SfxHit() => PlayOneShot(hitClip);
    public void SfxAttack() => PlayOneShot(attackClip);
    public void SfxDie()
    {
        amb.Stop();
        amb.volume = 0.1f;
        PlayOneShot(deathClip);
    }

    /* ─────────── 내부 재생 헬퍼 ─────────── */
    void PlayIdleLoop()
    {
        if (!idleWalkLoop || amb == null) return;
        amb.clip = idleWalkLoop;
        amb.pitch = Random.Range(.95f, 1.05f);
        amb.volume = .2f;
        amb.Play();
    }

    void PlayChaseLoop()
    {
        if (!chaseLoop || amb == null) return;
        amb.clip = chaseLoop;
        amb.pitch = Random.Range(.95f, 1.05f);
        amb.volume = .3f;
        amb.Play();
    }

    void PlayOneShot(AudioClip clip)
    {
        if (!clip) return;
        sfx.pitch = Random.Range(.95f, 1.05f);
        sfx.PlayOneShot(clip);
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
