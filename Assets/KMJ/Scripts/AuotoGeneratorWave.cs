using System.Collections;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(GeneratorTrigger))]
public class AutoGeneratorWave : NetworkBehaviour
{
    [SerializeField] float delaySec = 5f;   // ★ 지연 시간 (초)

    GeneratorTrigger generator;

    public override void Spawned()
    {
        if (!HasStateAuthority) return;     // Host/Server 에서만
        generator = GetComponent<GeneratorTrigger>();
        StartCoroutine(AutoTrigger());
    }

    IEnumerator AutoTrigger()
    {
        yield return new WaitForSeconds(delaySec);
        Debug.Log($"[AutoGenWave] {delaySec}s 후 자동 웨이브 트리거");
        generator.Interact();               // 발전기 강제 작동 → 웨이브 발생
    }
}
