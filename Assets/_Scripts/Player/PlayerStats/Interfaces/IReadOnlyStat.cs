using System;
using System.Collections.Generic;
using UnityEngine;

public interface IReadOnlyStat
{
    event Action? OnValueChanged;

    IReadOnlyList<IReadOnlyStat> Stats { get; }

    float BaseValue { get; }
    float FinalValue { get; }
}

public interface IReadOnlyStat<T> : IReadOnlyStat where T : struct, IStatModifierData<T>
{
    new IReadOnlyList<IReadOnlyStat<T>> Stats { get; }

    int ModifiersCount { get; }

    void GetModifiers(IList<StatModifier<T>> results);
}
