using UnityEngine;

public static class DamageCalculator
{
    public static void Apply(DamageInfo info, StatController target)
    {
        float defense = target.GetValue(StatType.Defense);
        float defReduction = target.GetValue(StatType.DefenseReduction);
        float effectiveDefense = defense * (1f - Mathf.Clamp01(defReduction))
                                         * (1f - Mathf.Clamp01(info.ArmorPen));

        float mitigation = effectiveDefense / (effectiveDefense + 100f); // tune the 100
        float finalDamage = info.Power * (1f - mitigation);

        target.TakeDamage(finalDamage);
        Debug.Log($"Applied damage: {finalDamage}");

        // Combat state: dealing OR taking damage flags both parties.
        info.Source?.GetComponent<ResourceRegen>()?.RegisterCombat();
        target.GetComponent<ResourceRegen>()?.RegisterCombat();
    }
}
