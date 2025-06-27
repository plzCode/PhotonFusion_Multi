using UnityEngine;

public class ZombieSpawnPivot : MonoBehaviour
{
    [Tooltip("게임 시작 시 소환할 좀비 수")]
    public int minPerPlayer = 1;
    public int maxPerPlayer = 3;

    [Tooltip("N마리를 퍼뜨릴 반경 (m)")]
    public float groupRadius = 5f;

    // 에디터에서 Gizmo로 확인용
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, groupRadius);
    }
}
