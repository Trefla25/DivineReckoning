using System;
using UnityEngine;

[Serializable]
public abstract class AbilityEffect
{
    public abstract void Execute(GameObject caster, GameObject target);
}

[Serializable]
public class DamageEffect : AbilityEffect
{
    public float baseAmount = 20f;
    [Tooltip("Adds caster Power * this to the hit (GAS-lite coefficient).")]
    public float powerScaling = 1f;
    [Range(0f, 1f)] public float armorPen;

    public override void Execute(GameObject caster, GameObject target)
    {
        if (!target.TryGetComponent(out StatController targetStats)) return;

        float power = baseAmount;
        if (caster.TryGetComponent(out StatController casterStats))
            power += casterStats.GetValue(StatType.Power) * powerScaling;

        DamageCalculator.Apply(new DamageInfo(power, armorPen, caster), targetStats);
    }
}

[Serializable]
public class ResourceGainEffect : AbilityEffect 
{
    [Tooltip("Resource granted to the CASTER when the ability resolves (e.g. 20 rage).")]
    public float amount = 20f;

    public override void Execute(GameObject caster, GameObject target)
    {
        // Route through ResourceRegen so the per-class mode gate applies (rage builds;
        // passive mana/energy pools correctly ignore the gain) AND the in-combat timer
        // refreshes. Fallback to a direct grant if there's no ResourceRegen component.
        if (caster.TryGetComponent(out ResourceRegen regen))
            regen.RegisterCombat(amount);
        else if (caster.TryGetComponent(out StatController stats))
            stats.GainResource(amount);
    }
}

[Serializable]
public class HealEffect : AbilityEffect
{
    public float amount = 15f;

    public override void Execute(GameObject caster, GameObject target)
    {
        if (target.TryGetComponent(out StatController stats)) stats.Heal(amount);
    }
}

[Serializable]
public class ApplyStatusEffect : AbilityEffect
{
    public StatusEffectData effect;   // reuses your existing buff/debuff system

    public override void Execute(GameObject caster, GameObject target)
    {
        if (effect == null) return;
        if (target.TryGetComponent(out StatusEffectController sec)) sec.Apply(effect);
    }
}
