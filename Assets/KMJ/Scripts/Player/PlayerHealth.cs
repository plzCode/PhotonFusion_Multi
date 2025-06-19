using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Networked] public int CurrentHP { get; private set; }
    public int maxHP = 100;

    public override void Spawned() => CurrentHP = maxHP;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_TakeDamage(int dmg)
    {
        Debug.Log($"[PlayerHealth] RPC_TakeDamage 받은 값 = {dmg}");
        if (CurrentHP <= 0) return;
        CurrentHP = Mathf.Max(CurrentHP - dmg, 0);
        Debug.Log($"[Player] HP → {CurrentHP}");
    }
}