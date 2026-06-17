using UnityEngine;

public enum StatType
{
    MaxHealth,
    MaxMana,
    Power,
    Defense,
    AttackSpeed,
    MoveSpeed,
    ArmorPen,          // 0..1 fraction of target defense ignored
    DefenseReduction,  // 0..1, a debuff applied TO a target's defense
    // --- optional, add when you need them ---
    // CritChance, CritDamage, CooldownReduction, LifeSteal, ManaRegen, HealthRegen,
}
