using UnityEngine;
using Fusion;

// PlayerAttackTester.cs (카메라에서 마우스 좌클릭 시 데미지 20)
public class PlayerAttackTester : NetworkBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] int damage = 20;

    void Update()
    {
        if (!Object.HasInputAuthority) return;         // 내 클라이언트만 입력
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                                out RaycastHit hit, 40f))
            {
                var z = hit.collider.GetComponentInParent<ZombieController>();
                if (z != null)
                    z.RPC_TakeDamage(damage);
            }
        }
    }
}
