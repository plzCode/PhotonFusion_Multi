using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHP = 100;

    [Networked] public int CurrentHP { get; set; }
    public int MaxHP => maxHP;
    public override void Spawned()
    {
        if (HasStateAuthority)                // Host/Server 쪽
            CurrentHP = maxHP;
    }

    /* 나중에 실제 데미지 주면 ↓ 호출 */
    public void TakeDamage(int dmg)
    {
        if (!HasStateAuthority) return;      // 서버에서만 체력 감소
        CurrentHP = Mathf.Max(CurrentHP - dmg, 0);
    }
}