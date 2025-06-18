using UnityEngine;

public class ZombieDetection : MonoBehaviour
{
    [Header("시야 감지")]
    public float visionRange = 15f;   // 최소 버전: 15 m
    public float visionAngle = 60f;   // 시야 각도

    [Header("청각 감지")]
    public float hearingRange = 20f;  // 소리 반경

    // 플레이어 Transform만 캐싱
    Transform player;
    void Awake() => player = GameObject.FindGameObjectWithTag("Player")?.transform;

    /* 플레이어를 볼 수 있나? */
    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dir = player.position - transform.position;
        if (dir.magnitude > visionRange) return false;

        if (Vector3.Angle(transform.forward, dir) > visionAngle * 0.5f)
            return false;

        // 장애물 체크
        if (Physics.Raycast(transform.position + Vector3.up * 1.2f,
                            dir.normalized, out RaycastHit hit, visionRange))
            return hit.transform.CompareTag("Player");

        return false;
    }

    /* 소리가 들리는가? */
    public bool CanHearSound(Vector3 soundPos)
    {
        return Vector3.Distance(transform.position, soundPos) <= hearingRange;
    }
}
