using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NetworkObject))]
public class ZombieWaveManager : NetworkBehaviour
{
    [Header("프리팹 & 풀")]
    [SerializeField] private NetworkObject zombiePrefab;          // ← ZombieBase 프리팹
    [SerializeField] private List<ZombieConfig> commonPool;       // Inspector에서 SO 드래그

    [Header("웨이브 간격")]
    [SerializeField] float minInterval = 6f;   // tension 1 일 때
    [SerializeField] float maxInterval = 20f;  // tension 0 일 때
    [SerializeField] float firstDelay = 5f;

    [Header("긴장도(tension) 계산")]
    [SerializeField] private float enemyCoeff = 0.05f;   // 적 1마리당 긴장도 가중치
    [SerializeField] private float smoothAlpha = 0.25f;    // 지수이동평균
    [SerializeField] private float tensionThreshold = 0.6f; // 이 값 이상이면 웨이브 정지

    [Header("스폰 반경 (플레이어 기준)")]
    [SerializeField] private float minDist = 10f;
    [SerializeField] private float maxDist = 20f;
    [SerializeField] private float navSampleRadius = 3f;

    [Header("스페셜 좀비 제한")]
    [SerializeField] int maxSpecialPerPlayer = 1;  // 플레이어당 허용 스페셜 수
    [Networked] public float tensionLevel { get; private set; }

    float waveTimer;
    float zombiesPerWaveScale = 1f;      // 웨이브 크기 스케일

#if UNITY_EDITOR
    /* (디버그용) 최근 스폰 좌표 기록 : Scene 뷰 Gizmo 확인 */
    const int HISTORY_MAX = 100;
    readonly Queue<(Vector3 pos, bool special)> spawnHistory = new();
#endif


    /* ─────────────────────────────────────────── */
    public override void Spawned()
    {
        if (!HasStateAuthority) { enabled = false; return; }
        waveTimer = -firstDelay;               // 처음엔 음수로 두었다가 0 → 첫 웨이브
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        /* 1) tension 업데이트 */
        tensionLevel = Mathf.Lerp(tensionLevel, CalcTension(), smoothAlpha);

        /* 2) 현재 인터벌 */
        float curInterval = Mathf.Lerp(maxInterval, minInterval, tensionLevel);

        /* 3) 웨이브 조건 */
        waveTimer += Runner.DeltaTime;
        if (waveTimer >= curInterval && tensionLevel < tensionThreshold)
        {
            waveTimer = 0;
            SpawnWave();
        }
    }

    /*──────── 키 입력: 웨이브 수 스케일 조절 ────────*/
    void Update()
    {
        if (!HasStateAuthority) return;

        if (Input.GetKeyDown(KeyCode.Minus))      // ‘-’ 키 → 웨이브 절반
            zombiesPerWaveScale = Mathf.Max(0.25f, zombiesPerWaveScale * 0.5f);

        if (Input.GetKeyDown(KeyCode.Equals))     // ‘=’(+) 키 → 2배
            zombiesPerWaveScale = Mathf.Min(1f, zombiesPerWaveScale * 2f);
    }

    /* ───────── 웨이브 생성 ───────── */
    private void SpawnWave()
    {
        var player = GameObject.FindWithTag("Player")?.transform;
        if (player == null || commonPool.Count == 0) return;
        /* 10~30 랜덤 → 스케일 적용 */
        int baseCount = Random.Range(10, 31);
        int spawnCount = Mathf.RoundToInt(baseCount * zombiesPerWaveScale);

        /* 현재 스페셜 수 & 최대 허용 계산 */
        int aliveSpecial = 
            FindObjectsByType<ZombieController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Count(z => z.Data.specialType != SpecialType.None);
        int maxAllowed =
            maxSpecialPerPlayer * GameObject.FindGameObjectsWithTag("Player").Length;

        for (int i = 0; i < spawnCount; i++)
        {
            /* ▶ 랜덤 SO 하나 고르기 */
            ZombieConfig cfg = commonPool[Random.Range(0, commonPool.Count)];

            /* ❷ 스페셜 제한 검사 */
            if (cfg.specialType != SpecialType.None && aliveSpecial >= maxAllowed)
            {
                i--;                   // 루프 다시 시도
                continue;
            }

            Vector3 pos = RandomPointAround(player.position);

            Runner.Spawn(zombiePrefab, pos, Quaternion.identity,
                onBeforeSpawned: (runner, obj) =>
                {
                    obj.GetComponent<ZombieController>().Init(cfg);
                });

            if (cfg.specialType != SpecialType.None)
                aliveSpecial++;        // 카운트 증가

#if UNITY_EDITOR
            /* 디버그 기록 */
            spawnHistory.Enqueue((pos, cfg.specialType != SpecialType.None));
            if (spawnHistory.Count > HISTORY_MAX) spawnHistory.Dequeue();
#endif
        }

        Debug.Log($"[Wave] Spawned {spawnCount} zombies");

    }

    /* ───────── 플레이어 주변 임의 NavMesh 지점 ───────── */
    private Vector3 RandomPointAround(Vector3 center)
    {
        for (int t = 0; t < 10; t++)
        {
            Vector2 dir2 = Random.insideUnitCircle.normalized;
            Vector3 raw = center + new Vector3(dir2.x, 0, dir2.y) *
                           Random.Range(minDist, maxDist);

            if (NavMesh.SamplePosition(raw, out NavMeshHit hit, 3f, NavMesh.AllAreas) &&
                !VisibilityUtil.IsInAnyPlayerView(hit.position))
            {
                return hit.position;
            }
        }
        return center; // 실패 시
    }
    /* ───────── 긴장도 계산 ───────── */
    float CalcTension()
    {
        float highest = 0;
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var hp = player.GetComponent<PlayerHealth>();
            if (hp == null) continue;

            float healthStress = 1f - (float)hp.CurrentHP / hp.MaxHP;

            int nearby = 
                GameObject.FindGameObjectsWithTag("Zombie").Count(z =>Vector3.Distance(z.transform.position, player.transform.position) < 15f);
            
            // 주변 좀비 수에 따른 긴장도
            float enemyStress = Mathf.Clamp01(nearby * enemyCoeff);

            highest = Mathf.Max(highest, healthStress + enemyStress);
        }
        return Mathf.Clamp01(highest);
    }
#if UNITY_EDITOR
    /* ───────── 최근 스폰 좌표 Gizmo ───────── */
    void OnDrawGizmos()
    {
        // 게임이 완전히 시작되고 Runner가 준비된 뒤에만 그리기
        if (!Application.isPlaying || waveTimer < 0) return;

        foreach (var (pos, special) in spawnHistory)
        {
            Gizmos.color = special ? Color.red : Color.cyan;
            Gizmos.DrawSphere(pos + Vector3.up * 0.3f, 0.25f);

            if (special)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(pos + Vector3.up * 0.3f, 0.35f);
            }
        }
    }
#endif

    /* ───────── 이벤트 웨이브 트리거 ───────── */
    public void TriggerEventWave(int count)
    {
        if (!HasStateAuthority) return;

        Transform player = GameObject.FindWithTag("Player")?.transform;
        if (player == null || commonPool.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            ZombieConfig cfg = commonPool[Random.Range(0, commonPool.Count)];
            Vector3 pos = RandomPointAround(player.position);
            Runner.Spawn(zombiePrefab, pos, Quaternion.identity,
                onBeforeSpawned: (r, obj) =>
                    obj.GetComponent<ZombieController>().Init(cfg));
        }
        Debug.Log($"[EventWave] {count} zombies spawned");
    }
}
