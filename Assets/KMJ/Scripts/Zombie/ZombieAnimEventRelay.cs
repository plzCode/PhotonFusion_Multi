using UnityEngine;

public class ZombieAnimEventRelay : MonoBehaviour
{
    ZombieAIController ctrl;

    void Awake()               // 좀비 프리팹이 스폰될 때 한 번만 실행
    {
        ctrl = GetComponentInParent<ZombieAIController>();
    }

    // ─── Animation Event 에서 호출할 메서드 ───
    public void OnAttackHitEvent()     // Animation Event에서 호출
    {
        if (ctrl != null && ctrl.IsAttacking)
            ctrl.HandleAttackHit();

    }
}
