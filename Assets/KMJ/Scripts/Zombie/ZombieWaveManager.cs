using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NetworkObject))]
public class ZombieWaveManager : NetworkBehaviour
{
    [Header("공통 프리팹")]
    [SerializeField] private NetworkObject zombiePrefab;          // ← ZombieBase 프리팹

    [Header("일반 좀비 풀")]
    [SerializeField] List<ZombieConfig> commonPool = new();
    [SerializeField] int commonCount = 5;   // 웨이브당 일반 좀비 수

    //[Header("특수 좀비 SO (플레이어 수 1:1)")]
    //[SerializeField] ZombieConfig flashCfg;   // Flash
    //[SerializeField] ZombieConfig slowCfg;    // Slow
    //[SerializeField] ZombieConfig alarmCfg;   // Alarm
    ////더 추가할 예정

    [Header("웨이브 간격(긴장도 0↔1)")]
    [SerializeField] float maxInterval = 20f;
    [SerializeField] float minInterval = 6f;
    [SerializeField] float firstDelay = 5f;

    [Header("자동 웨이브 토글")]
    [SerializeField] bool autoWave = false;    // Inspector에서 ON/OFF

    [Header("긴장도(tension) 계산")]
    [SerializeField] private float enemyCoeff = 0.05f;   // 적 1마리당 긴장도 가중치
    [SerializeField] private float smoothAlpha = 0.25f;    // 지수이동평균
    //[SerializeField] private float tensionThreshold = 0.6f; // 이 값 이상이면 웨이브 정지

    [Header("스폰 반경 (플레이어 기준)")]
    [SerializeField] private int minDist = 20;
    [SerializeField] private int maxDist = 30;
    //[SerializeField] private float navSampleRadius = 3f;
   
    /*──────── Networked ────────*/
    [Networked] public float tensionLevel { get; private set; }

    float waveTimer;



    /* ─────────────────────────────────────────── */
    public override void Spawned()
    {
        if (!HasStateAuthority) { enabled = false; return; }
        waveTimer = -firstDelay;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        /* 1) tension 업데이트 */
        tensionLevel = Mathf.Lerp(tensionLevel, CalcTension(), smoothAlpha);

        /* ── 자동 웨이브 ── */
        if (!autoWave) return;

        float curInterval = Mathf.Lerp(maxInterval, minInterval, tensionLevel);
        waveTimer += Runner.DeltaTime;

        if (waveTimer >= curInterval && tensionLevel < 0.6f)
        {
            waveTimer = 0;
            SpawnWave(commonCount);
        }
    }
    public void SpawnWave(int amount)
    {
        // 1) GameManager에서 현재 접속 플레이어 목록 받기
        var players = GameManager.Players;          // ↖ 기존 컬렉션 그대로 사용

        // 2) 플레이어가 없으면 더미 기준점 하나 마련
        Vector3 fallbackPos = transform.position;   // WaveManager 위치(씬 중앙 등)

        for (int i = 0; i < amount; i++)
        {
            // 3) 기준점 선택
            Vector3 center;
            if (players.Count > 0)
            {
                // 랜덤 플레이어 하나 뽑아 20~30 m 밖
                var pl = players[Random.Range(0, players.Count)];
                center = pl ? pl.transform.position : fallbackPos;
            }
            else
            {
                // 플레이어가 전혀 없으면 매니저 위치 기준
                center = fallbackPos;
            }

            // 4) NavMesh 위 20~30 m 위치 샘플링
            Vector3 pos = RandomNavPointOutside(center, 20f, 30f);
            if (pos == Vector3.zero)         // 샘플 실패 시 바로 옆에라도
                pos = center + Vector3.forward * 22f;

            // 5) 일반 좀비만 스폰
            Runner.Spawn(zombiePrefab, pos, Quaternion.identity);
        }
    }

    /* 공통 스폰 함수 */
    void SpawnBatch(int count, IEnumerable<ZombieConfig> pool, PlayerController[] players)
    {
        if (count <= 0 || pool == null) return;

        foreach (var cfg in pool)
        {
            for (int i = 0; i < count; i++)
            {
                /* ① 기준점: 플레이어 중 하나, 없으면 WaveManager 위치 */
                Vector3 center;
                if (players.Length > 0 && players[0] != null)
                {
                    var p = players[Random.Range(0, players.Length)];
                    center = p ? p.transform.position : transform.position;
                }
                else
                {
                    center = transform.position;
                }

                /* ② 20 ~ 30 m NavMesh 지점 */
                Vector3 pos = RandomNavPointOutside(center, 20f, 30f);

                /* ③ 일반 좀비 프리팹 스폰 */
                Runner.Spawn(zombiePrefab, pos, Quaternion.identity,
                    onBeforeSpawned: (r, obj) =>
                        obj.GetComponent<ZombieController>().Init(cfg));
            }
        }
    }

    /* NavMesh 지점 함수 – 20~30 m 밖에서만 */
    Vector3 RandomNavPointOutside(Vector3 center, float min, float max)
    {
        for (int t = 0; t < 10; t++)
        {
            Vector3 dir = Random.onUnitSphere; dir.y = 0;
            Vector3 raw = center + dir.normalized * Random.Range(min, max);
            if (NavMesh.SamplePosition(raw, out var hit, 5f, NavMesh.AllAreas))
                return hit.position;
        }
        return Vector3.zero;   // 실패
    }

    /* ───────── 긴장도 계산 ───────── */
    float CalcTension()
    {
        float highest = 0;
        foreach (var p in FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            float hpStress = 1f - (float)p.Hp / 100; // 플레이어 HP 비율 (0~1) maxHp 같은 변수 생기면 고치기

            int nearZ = GameObject.FindGameObjectsWithTag("Zombie").Count(z => Vector3.Distance(z.transform.position, p.transform.position) < 15f);

            float enemyStress = Mathf.Clamp01(nearZ * enemyCoeff);

            highest = Mathf.Max(highest, hpStress + enemyStress);
        }
        return Mathf.Clamp01(highest);
    }

    /* 외부 이벤트 웨이브용 public 메서드 */
    public void TriggerEventWave(int count)
    {
        if (!HasStateAuthority) return;
        var players = FindObjectsByType<PlayerController>(
                          FindObjectsInactive.Exclude,
                          FindObjectsSortMode.None);
        if (players.Length == 0) return;

        SpawnBatch(count, commonPool, players);
        Debug.Log($"[EventWave] {count} zombies spawned");
    }

}
