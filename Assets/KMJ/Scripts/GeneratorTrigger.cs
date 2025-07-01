using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class GeneratorTrigger : NetworkBehaviour
{
    [Header("Wave Settings")]
    public string waveId = "Generator";     // 이 발전기와 연결된 웨이브 이름
    public List<WaveConfig> waveCfg = new List<WaveConfig>();              // 웨이브 규모·횟수 설정
    public ZombieWaveManager waveMgr;       // 씬의 WaveManager Drag

    bool activated;

    /// <summary>발전기를 작동시켜 웨이브를 호출</summary>
    public void Interact()
    {
        /*if (activated || !HasStateAuthority) return;
        activated = true;

        

        if (waveMgr == null)
            Debug.LogWarning("[GeneratorTrigger] WaveManager가 할당되지 않았습니다.");
        else
            waveMgr.TriggerEventWave(waveId, waveCfg, true);*/
    }
    public void Interact(string _waveId)
    {
        if (!HasStateAuthority) return;

        foreach (var cfg in waveCfg)
        {
            if (cfg.waveId == _waveId)
            {
                waveMgr.TriggerEventWave(_waveId, cfg, true);
                return;
            }
        }
        /*if (waveMgr == null)
            Debug.LogWarning("[GeneratorTrigger] WaveManager가 할당되지 않았습니다.");
        else
            waveMgr.TriggerEventWave(_waveId, waveCfg, true);*/
    }
}
