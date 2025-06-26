using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // 따라갈 대상 (플레이어)
    public Vector3 offset = new Vector3(0, 5, -7); // 카메라와의 거리

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target); // 타겟을 바라보게
        }
    }
}
