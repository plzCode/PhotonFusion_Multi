using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.AI;


public class ZombieWaveManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject commonPrefab;

    [Header("Default WaveConfig")]
    public WaveConfig defaultCfg;

    /* ───────── 게임 시작 1회 초기 스폰 ───────── */
    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        var pivots = FindObjectsByType<ZombieSpawnPivot>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None).ToList();
        Debug.Log($"[WM] Spawned(), pivots:{pivots.Count}");

        foreach (var pv in pivots)
        {
            int players = Mathf.Max(1, Runner.ActivePlayers.Count());
            int cnt = Random.Range(pv.minPerPlayer, pv.maxPerPlayer + 1) * players;

            SpawnGroup(commonPrefab, cnt,
                       pv.transform.position,
                       0f, pv.groupRadius,
                       false);
        }
    }

    // (b) WaveConfig + waveId : 트리거·객체별 커스텀
    public void TriggerEventWave(string id, WaveConfig cfg, bool forceChase = true)
    {
        if (!HasStateAuthority) return;
        if (cfg.maxTriggerTimes > 0 && cfg.triggered >= cfg.maxTriggerTimes) return;

        var points = FindObjectsByType<EventSpawnPoint>(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None)
                    .Where(p => p.waveId == id)
                    .ToArray();

        int players = Mathf.Max(1, Runner.ActivePlayers.Count());
        int total = Random.Range(cfg.minCount, cfg.maxCount + 1) * players;
        int perPt = Mathf.CeilToInt((float)total / Mathf.Max(1, points.Length));

        Debug.Log($"[WaveMgr] FixedWave '{id}' {total} zombies");

        foreach (var pt in points)
            SpawnGroup(commonPrefab, perPt,
                       pt.transform.position,
                       cfg.innerRadius, cfg.outerRadius,
                       forceChase);

        //cfg.triggered++;
    }



    /* ───────── 지역 리스폰 (필요한 경우 쓰기) ───────── */
    public void RespawnArea(ZombieSpawnPivot pivot)
    {
        if (!HasStateAuthority) return;
        int playerCnt = GameManager.Players.Count;
        int cnt = Random.Range(pivot.minPerPlayer, pivot.maxPerPlayer + 1) * playerCnt;
        SpawnGroup(commonPrefab, cnt, pivot.transform.position, 0f, pivot.groupRadius, false);
    }

    void SpawnGroup(GameObject prefab, int n,
                Vector3 center, float innerR, float outerR, bool forceChase)
    {
        for (int i = 0; i < n; i++)
        {
            if (!HasStateAuthority) return;
            // 🔹 각 좀비마다 반경 내 랜덤 오프셋
            Vector2 cir = Random.insideUnitCircle.normalized *
                          Random.Range(innerR, outerR);          // ← 추가
            Vector3 raw = center + new Vector3(cir.x, 0f, cir.y);

            if (!NavMesh.SamplePosition(raw, out var hit, 2f, NavMesh.AllAreas))
                continue;

            NetworkObject zombie = Runner.Spawn(prefab, hit.position, Quaternion.identity,
                onBeforeSpawned: (r, obj) =>
                {
                    if (forceChase)
                        obj.GetComponent<ZombieAIController>()
                           .SpawnAggro(center);   // 목표는 웨이브 중심
                });

            if (zombie.GetComponent<NavMeshAgent>() != null)
            {
                
                zombie.GetComponent<NavMeshAgent>().Warp(hit.position);
            }


            Debug.Log($"[WM] SpawnGroup() {i + 1}/{n} @ {hit.position} ({innerR} ~ {outerR})");
        }
    }
}
