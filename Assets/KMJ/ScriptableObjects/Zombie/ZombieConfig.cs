using UnityEngine;

[CreateAssetMenu(fileName = "ZombieConfig",
                 menuName = "Game/Zombie Config", order = 0)]
public class ZombieConfig : ScriptableObject
{
    [Header("공통 스탯")]
    public string displayName = "Common Zombie";
    public int maxHP = 100;
    public int damage = 10;
    public float attackRate = 1.0f;   //공격속도
    public float moveSpeed = 2.5f;
    public float attackRange = 1.3f;
    public float detectRange = 10f;

    public SpecialType specialType = SpecialType.None;

    [Header("특수 타입 세부 파라미터")]
    public float radius = 5f;   // AOE 범위용(Flash, Slow, Disarm, Alarm 등)
    public float duration = 2f;   // 효과 지속
    public float extraMultiplier = 1.5f; // Buffed 배율 등
}

public enum SpecialType
{
    None,        // 일반 좀비
    Enforce,      // 체력·대미지 배율
    Flash,       // 실명
    Slow,        // 둔화
    Disarm,      // 무기 사용 불가
    Alarm,      // 살아있는 동안 소환
    Infector,    // 감염(일정 시간 내에 약을 복용하지 않으면 좀비화(죽음?))
}