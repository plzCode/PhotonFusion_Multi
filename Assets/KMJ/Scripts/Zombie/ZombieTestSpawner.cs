using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class ZombieTestSpawner : NetworkBehaviour
{
    [SerializeField] NetworkObject zombiePrefab;
    [SerializeField] float spawnDistance = 8f;      // 플레이어와 최소 거리
    [SerializeField] float navSampleRadius = 3f;    // NavMesh 투영 반경

    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        // 1) 현재 씬에 존재하는 첫 번째 Player 찾기
        Transform player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) { Debug.LogWarning("No player yet"); return; }

        // 2) 플레이어 주위를 랜덤한 방향으로 spawnDistance 만큼 오프셋
        Vector2 rnd2D = Random.insideUnitCircle.normalized;         // XY 랜덤 유닛벡터
        Vector3 rawPos = player.position +
                         new Vector3(rnd2D.x, 0, rnd2D.y) * spawnDistance;

        // 3) NavMesh 표면으로 투영
        if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
            rawPos = hit.position;
        else
            Debug.LogWarning("NavMesh.SamplePosition failed, using rawPos");

        // 4) 스폰
        Runner.Spawn(zombiePrefab, rawPos, Quaternion.identity);
    }
}
