using System;
using UnityEngine;

public class StatResource
{
    private readonly Stat maxStat;
    public float Current { get; private set; }
    public float Max => maxStat.FinalValue;

    public event Action<float, float>? OnChanged; // (current, max)

    public StatResource(Stat maxStat, float startAt = -1f)
    {
        this.maxStat = maxStat;
        Current = startAt < 0 ? maxStat.FinalValue : startAt;
        maxStat.OnValueChanged += () => Set(Current); // re-clamp + notify when max changes
    }

    public void Set(float value)
    {
        Current = Math.Clamp(value, 0, Max);
        OnChanged?.Invoke(Current, Max);
    }

    public void Modify(float delta) => Set(Current + delta);
    public bool IsEmpty => Current <= 0;
}
