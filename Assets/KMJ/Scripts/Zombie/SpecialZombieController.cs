using Fusion;
using UnityEngine;

 
[RequireComponent(typeof(ZombieController))]
public class SpecialZombieController : NetworkBehaviour
{
    ZombieConfig cfg;
    ZombieAIController ai;


    /* ZombieController.Init() 안에서 호출 */
    public void Init(ZombieConfig config)
    {
        cfg = config;
        ai = GetComponent<ZombieAIController>();

        if (cfg.specialType == SpecialType.Alarm && HasStateAuthority)
        {
            InvokeRepeating(nameof(CallMiniWave), 5f, 7f);
        }
    }

    /* ───────── 특수 효과 구현 ───────── */
    // Flash : 섬광
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Flash(Vector3 origin, float radius, float duration, RpcInfo info = default)
    {
        var pm = PlayerMovement.Local;
        if (!pm) return;

        Transform cam = pm.transform;          // 플레이어 시야 기준
        /* (1) 거리 */
        if (Vector3.Distance(cam.position, origin) > radius) return;

        /* (2) 시야 각도 90° */
        Vector3 dir = (origin - cam.position).normalized;
        if (Vector3.Dot(cam.forward, dir) < 0.0f) return;   // 뒤를 보고 있으면 무시

        FlashScreen.Instance?.Blink(duration);
    }

    // Slow : 이동 속도 감소
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Slow(Vector3 origin, float radius, float factor, float time, RpcInfo info = default)
    {
        var pm = PlayerMovement.Local;
        if (!pm) return;

        if (Vector3.Distance(pm.transform.position, origin) > radius) return;
        pm.ApplySlow(factor, time);
    }

    // Alarm : 소규모 웨이브 호출
    void CallMiniWave()
    {
        var wm = UnityEngine.Object.FindFirstObjectByType<ZombieWaveManager>();
        if (wm && wm.enabled)
            wm.TriggerEventWave(Mathf.RoundToInt(cfg.radius)); // radius 값을 소환 수로 사용
    }
    /* ───────── 좀비가 죽을 때 호출 ───────── */
    public void OnDeath()
    {
        switch (cfg.specialType)
        {
            case SpecialType.Flash:
                RPC_Flash(transform.position, cfg.radius, cfg.duration);
                break;
            case SpecialType.Slow:
                RPC_Slow(transform.position, cfg.radius, cfg.slowFactor, cfg.duration);
                break;
            case SpecialType.Alarm:
                CancelInvoke(nameof(CallMiniWave));
                break;
            // Enforce·Disarm·Infector 등은 나중에 확장
        }
    }
}
