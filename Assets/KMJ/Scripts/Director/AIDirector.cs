using Fusion;
using UnityEngine;

/// 최소 기능: 씬이 시작되면 서버가 좀비 한 마리만 스폰
public class AIDirector : NetworkBehaviour
{
    [Header("테스트용 좀비 Prefab")]
    public NetworkPrefabRef zombiePrefab;

    private bool spawnedOnce = false;

    // NetworkBehaviour 라이프사이클: Spawned()는 네트워크 객체가 생기자마자 호출
    public override void Spawned()
    {
        // 서버(Host)일 때만 작업
        if (!HasStateAuthority) return;

        // 한 번만 스폰
        if (!spawnedOnce)
        {
            spawnedOnce = true;

            // AIDirector 자기 위치에서 +5m 앞에 소환
            Vector3 pos = transform.position + Vector3.forward * 5f;

            Runner.Spawn(zombiePrefab, pos, Quaternion.identity);
        }
    }
}