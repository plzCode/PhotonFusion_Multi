using Fusion;
using UnityEngine;

public class AIDirector : NetworkBehaviour
{
    [Header("Wave Manager")]
    [SerializeField] ZombieWaveManager waveMgr;

    [Header("Tension")]
    [Networked] public float tension { get; set; }
    [SerializeField] float tensionPerHit = 0.05f;
    [SerializeField] float tensionDecaySec = 0.02f;   // 초당 감소량

    [Header("Auto Wave")]
    [SerializeField] float thresh = 0.8f;             // 0.0~1.0
    [SerializeField] string autoWaveId = "Auto";

    void Update()
    {
        if (!HasStateAuthority) return;

        // 긴장도 자연 감소
        tension = Mathf.Max(0f, tension - tensionDecaySec * Time.deltaTime);

        // 예: 플레이어가 소음‧피격 일 때 AddTension() 호출
        //if (tension >= thresh)
        //{
        //    waveMgr.TriggerEventWave(autoWaveId, waveMgr.defaultCfg, true);
        //    tension = 0f;     // 발동 후 리셋
        //}
    }

    public void AddTension(float value)
    {
        if (!HasStateAuthority) return;
        tension = Mathf.Clamp01(tension + value);
    }
}