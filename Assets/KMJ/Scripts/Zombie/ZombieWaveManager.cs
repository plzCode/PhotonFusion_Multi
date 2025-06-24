using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NetworkObject))]
public class ZombieWaveManager : NetworkBehaviour
{
    [Header("공통 프리팹")]
    [SerializeField] private NetworkObject zombiePrefab;

    [Header("일반 좀비 풀")]
    [SerializeField] List<ZombieConfig> commonPool = new();

    //[Header("특수 좀비 SO (플레이어 수 1:1)")]
    //[SerializeField] ZombieConfig flashCfg;   // Flash
    //[SerializeField] ZombieConfig slowCfg;    // Slow
    //[SerializeField] ZombieConfig alarmCfg;   // Alarm
    ////더 추가할 예정


    [Header("웨이브 수량")]
    [SerializeField] int minCount = 10;   
    [SerializeField] int maxCount = 18;

    [Header("웨이브 간격(긴장도 0↔1)")]
    [SerializeField] float maxInterval = 20f;
    [SerializeField] float minInterval = 6f;
    [SerializeField] float firstDelay = 5f;

    [Header("자동 웨이브 ON/OFF")]
    [SerializeField] bool autoWave = true;

    [Header("긴장도 계산 옵션")]
    [SerializeField] float enemyCoeff = 0.05f;
    [SerializeField] float smoothAlpha = 0.25f;
    [SerializeField] private float tensionThreshold = 0.6f; // 이 값 이상이면 웨이브 정지

    [Header("플레이어 기준 스폰 반경(m)")]
    [SerializeField] float spawnRadiusMin = 12f;
    [SerializeField] float spawnRadiusMax = 25f;
    [SerializeField] float navSampleRadius = 3f;

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

        if (waveTimer >= curInterval && tensionLevel < tensionThreshold)
        {
            waveTimer = 0;
            SpawnWave();
        }
        waveTimer += Runner.DeltaTime;
    }
    void SpawnWave()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);

        if (players.Length == 0 || commonPool.Count == 0) return;

        int commonCount = Random.Range(minCount, maxCount + 1);
        SpawnBatch(commonCount, commonPool, players);
        Debug.Log($"[Wave] Spawned {commonCount} common zombies");
    }

    /* 공통 스폰 함수 */
    void SpawnBatch(int count, IEnumerable<ZombieConfig> pool, PlayerController[] players)
    {
        foreach (var cfg in pool)
        {
            for (int i = 0; i < count; i++)
            {
                Transform pivot = players[Random.Range(0, players.Length)].transform;
                Vector3 pos = RandomNavPointAround(pivot.position);

                Runner.Spawn(zombiePrefab, pos, Quaternion.identity,
                    onBeforeSpawned: (r, obj) =>
                        obj.GetComponent<ZombieController>().Init(cfg));
            }
        }
    }

    /* ───────── 플레이어 주변 임의 NavMesh 지점 ───────── */
    Vector3 RandomNavPointAround(Vector3 center)
    {
        for (int tries = 0; tries < 8; tries++)
        {
            Vector3 raw = center + Random.insideUnitSphere *
                          Random.Range(spawnRadiusMin, spawnRadiusMax);
            raw.y = center.y;

            if (NavMesh.SamplePosition(raw, out var hit, navSampleRadius, NavMesh.AllAreas))
                return hit.position;
        }
        return center;             // 실패 시 플레이어 위치(안전 장치)
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
