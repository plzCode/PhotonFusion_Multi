using UnityEngine;
using Fusion;

public class EventWaveTrigger : NetworkBehaviour
{
    [SerializeField] WaveConfig config;      
    [SerializeField] ZombieWaveManager wm;    
    [SerializeField] string waveId = "Generator";

    bool fired;

    public void Activate()
    {
        if (!HasStateAuthority || fired) return;
        fired = true;

        wm.TriggerEventWave(waveId, config, true);
    }
}
