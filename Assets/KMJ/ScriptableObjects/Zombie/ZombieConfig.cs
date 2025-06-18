using UnityEngine;

[CreateAssetMenu(fileName = "ZombieConfig",
                 menuName = "Game/Zombie Config", order = 0)]
public class ZombieConfig : ScriptableObject
{
    [Header("공통 스탯")]
    public string displayName = "Common Zombie";
    public int maxHP = 100;
    public int damage = 10;
    public float attackRate = 1.0f;   // 초당 공격 횟수
    public float moveSpeed = 2.5f;   // NavMeshAgent.speed
    public float attackRange = 1.3f;   // 근접 공격 거리
    public float detectRange = 10f;    // ZombieDetection.range

    /*──────── 특수 타입 ────────*/
    [Header("Special Ability")]
    public SpecialType specialType = SpecialType.None;

    [Tooltip("Flash/Slow 발동 반경 (m)")]
    public float radius = 5f;

    [Tooltip("Flash/Slow 지속시간 (초)")]
    public float duration = 2f;

    [Tooltip("Slow 배율 (0.6 = 40 % 감소)")]
    [Range(0.1f, 1f)] public float slowFactor = 0.6f;
}

public enum SpecialType
{
    None,     // 일반
    Flash,    // 죽을 때 화면 실명
    Slow,     // 죽을 때 이동 둔화
    Alarm,    // 살아 있는 동안 미니 웨이브 호출
    // Enforce / Disarm / Infector … 이후 확장 가능
}