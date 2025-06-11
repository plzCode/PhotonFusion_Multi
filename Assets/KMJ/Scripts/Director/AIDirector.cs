using Fusion;
using UnityEngine;

public class AIDirector : NetworkBehaviour
{
    [SerializeField] ZombieWaveManager waveMgr;  // Inspector ZombieWaveManager 연결

    enum Phase { Calm, BuildUp, Horde, Finale }
    [Networked] Phase CurrentPhase { get; set; }

    // 진행도·사운드·시간 등 추가 변수
    float calmTimer, buildUpTimer;
    [SerializeField] float buildUpCooldown = 10f;  // 웨이브 쿨다운(초)
    float buildUpWaveTimer;                        // 내부 타이머

    public override void Spawned()
    {
        if (!HasStateAuthority) { enabled = false; return; }
        CurrentPhase = Phase.Calm;
    }

    public override void FixedUpdateNetwork()
    {
        switch (CurrentPhase)
        {
            case Phase.Calm:
                if (waveMgr.tensionLevel < 0.3f)
                {
                    calmTimer += Runner.DeltaTime;
                    if (calmTimer > 15f)   // 15초 숨 돌렸으면
                    {
                        CurrentPhase = Phase.BuildUp;
                        calmTimer = 0;
                    }
                }
                else calmTimer = 0;
                break;

            case Phase.BuildUp:
                if (waveMgr.tensionLevel < 0.6f)
                {
                    buildUpTimer += Runner.DeltaTime;
                    buildUpWaveTimer += Runner.DeltaTime;

                    if (buildUpTimer > 5f &&            // 최소 5초 Build-Up 유지
                        buildUpWaveTimer > buildUpCooldown)
                    {
                        waveMgr.TriggerEventWave(Random.Range(10, 16));
                        buildUpWaveTimer = 0f;          // 쿨다운 리셋
                    }
                }
                else         // 플레이어가 힘들어함 → 다시 Calm
                {
                    CurrentPhase = Phase.Calm;
                    buildUpTimer = buildUpWaveTimer = 0f;
                }
                // 이벤트 웨이브 → Horde
                break;

            case Phase.Horde:
                // WaveManager.TriggerEventWave(40~50) 호출
                // tension 완전 가득 차면 잠시 쉬고 Phase.Calm
                if (waveMgr.tensionLevel > 0.8f)
                    CurrentPhase = Phase.Calm;
                break;

            case Phase.Finale:
                // 마지막 장면
                break;
        }
    }

    /* 외부 이벤트로 하드 웨이브 호출 */
    public void OnGeneratorRepaired()
    {
        CurrentPhase = Phase.Horde;
        waveMgr.TriggerEventWave(50);   // 대량
    }
}