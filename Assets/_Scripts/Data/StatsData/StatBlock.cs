using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatBlock
{
    [Serializable]
    public struct Entry
    {
        public StatType Type;
        public float Value;
    }

    [SerializeField] private List<Entry> entries = new();

    public IEnumerable<KeyValuePair<StatType, float>> ToPairs()
    {
        foreach (var e in entries)
            yield return new KeyValuePair<StatType, float>(e.Type, e.Value);
    }
}
