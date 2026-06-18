using System;
using System.Collections.Generic;
using UnityEngine;

public class StatSheet
{
    private readonly Dictionary<StatType, Stat> stats = new();

    public StatResource Health { get; }
    public StatResource Resource { get; }

    public StatSheet(IEnumerable<KeyValuePair<StatType, float>> baseValues)
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
            stats[type] = new Stat(0);

        foreach (var kvp in baseValues)
            stats[kvp.Key].BaseValue = kvp.Value;

        Health = new StatResource(stats[StatType.MaxHealth]);
        Resource = new StatResource(stats[StatType.MaxResource]);
    }

    public Stat Get(StatType type) => stats[type];
    public float Value(StatType type) => stats[type].FinalValue;

    public void AddModifier(StatType type, StatModifier<StatModifierData> mod) => stats[type].AddModifier(mod);
    public void RemoveSource(StatType type, object source) => stats[type].RemoveModifiersFromSource(source);

    // Remove a source from EVERY stat (an expiring buff that touched several).
    public void RemoveSourceEverywhere(object source)
    {
        foreach (var stat in stats.Values)
            stat.RemoveModifiersFromSource(source);
    }
}
