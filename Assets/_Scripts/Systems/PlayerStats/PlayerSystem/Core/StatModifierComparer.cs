using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public readonly struct StatModifierComparer<T> : IComparer<StatModifier<T>> where T : struct, IStatModifierData<T>
{
    public static readonly StatModifierComparer<T> Default = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int Compare(StatModifier<T> x, StatModifier<T> y) => x.CompareTo(y);
}
