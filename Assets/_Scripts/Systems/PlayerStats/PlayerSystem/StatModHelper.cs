using UnityEngine;

public static class StatModHelper
{
    public static StatModifier<StatModifierData> Flat(float v, object? source = null)
        => new(v, new StatModifierData(StatModifierType.Add, source));

    public static StatModifier<StatModifierData> PercentAdd(float v, object? source = null)
        => new(v, new StatModifierData(StatModifierType.Mult, source));

    public static StatModifier<StatModifierData> PercentMult(float v, object? source = null)
        => new(v, new StatModifierData(StatModifierType.MultTotal, source));

    public static StatModifier<StatModifierData> Floor(float v, object? source = null)
        => new(v, new StatModifierData(StatModifierType.Max, source));

    public static StatModifier<StatModifierData> Cap(float v, object? source = null)
        => new(v, new StatModifierData(StatModifierType.Min, source));
}
