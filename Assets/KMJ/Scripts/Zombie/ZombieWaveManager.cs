using System.Collections;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.AI;


public class ZombieWaveManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject commonPrefab;

    [Header("SFX")]
    [SerializeField] AudioClip eventWaveRoar;   // EventWaveSound.wav
    [SerializeField] float eventWaveVol = .9f;

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

        var points = FindObjectsByType<EventSpawnPoint>(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None)
                    .Where(p => p.waveId == id)
                    .ToArray();

        if (points.Length == 0)
        {
            Debug.LogWarning($"[WM] No spawn points for id:{id}");
            return;
        }

        /* ② 1회 or 지속 웨이브 분기 */
        if (!cfg.isContinuous)
        {
            SpawnOnce(points, cfg, forceChase);
        }
        else
        {
            StartCoroutine(ContinuousWave(points, cfg, forceChase));
        }
    }

    void SpawnOnce(EventSpawnPoint[] pts, WaveConfig cfg, bool forceChase)
    {
        int players = Mathf.Max(1, Runner.ActivePlayers.Count());
        int total = Random.Range(cfg.minEvnet, cfg.maxEvnet + 1) * players;
        int perPt = Mathf.CeilToInt((float)total / pts.Length);

        foreach (var pt in pts)
        {
            PlayRoar(pt.transform.position);                       // ★ 포효
            SpawnGroup(commonPrefab, perPt, pt.transform.position,
                       cfg.innerRadius, cfg.outerRadius, forceChase);
        }
    }

    IEnumerator ContinuousWave(EventSpawnPoint[] pts, WaveConfig cfg, bool forceChase)
    {
        float endTime = Time.time + cfg.duration;

        int players = Mathf.Max(1, Runner.ActivePlayers.Count());

        while (Time.time < endTime)
        {
            foreach (var pt in pts)
            {
                PlayRoar(pt.transform.position);                   // ★ 매 주기마다 포효
                SpawnGroup(commonPrefab,
                           Random.Range(cfg.minEvnet, cfg.maxEvnet + 1) * players,
                           pt.transform.position,
                           cfg.innerRadius, cfg.outerRadius, forceChase);
            }
            yield return new WaitForSeconds(cfg.interval);
        }
    }

    void PlayRoar(Vector3 pos)
    {
        if (eventWaveRoar)
            AudioSource.PlayClipAtPoint(eventWaveRoar, pos + Vector3.up * 1.5f, eventWaveVol);
    }

    /* ───────── 지역 리스폰 (필요한 경우 쓰기) ───────── */
    public void RespawnArea(ZombieSpawnPivot pivot)
    {
        if (!HasStateAuthority) return;
        int playerCnt = GameManager.Players.Count;
        int cnt = Random.Range(pivot.minPerPlayer + 1, pivot.maxPerPlayer + 1) * playerCnt;
        SpawnGroup(commonPrefab, cnt, pivot.transform.position, 0f, pivot.groupRadius, false);
    }

    void SpawnGroup(GameObject prefab, int n,
                Vector3 center, float innerR, float outerR, bool forceChase)
    {
        for (int i = 0; i < n; i++)
        {
            if (!HasStateAuthority) return;

            Vector2 cir = Random.insideUnitCircle.normalized *
                          Random.Range(innerR, outerR);
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

            //Debug.Log($"[WM] SpawnGroup() {i + 1}/{n} @ {hit.position} ({innerR} ~ {outerR})");
        }
    }
}
