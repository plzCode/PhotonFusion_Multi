using System.Collections.Generic;
using UnityEngine;

public static class VisibilityUtil
{
    /// <summary>어느 플레이어 카메라에도 완전히 보이면 true.</summary>
    public static bool IsInAnyPlayerView(Vector3 worldPos)
    {
        foreach (Camera cam in GetAllPlayerCameras())
        {
            Vector3 v = cam.WorldToViewportPoint(worldPos);

            bool inFront = v.z > 0f;
            bool inside = v.x >= 0 && v.x <= 1 && v.y >= 0 && v.y <= 1;

            if (inFront && inside)
            {
                // 벽 등의 장애물이 없는지 Raycast로 확인
                Vector3 dir = worldPos - cam.transform.position;
                if (!Physics.Raycast(cam.transform.position, dir.normalized,
                                     out RaycastHit hit, dir.magnitude,
                                     ~LayerMask.GetMask("Enemy")))
                    return true;
            }
        }
        return false;
    }

    private static IEnumerable<Camera> GetAllPlayerCameras()
    {
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            var cam = p.GetComponentInChildren<Camera>();
            if (cam) yield return cam;
        }
    }
}
