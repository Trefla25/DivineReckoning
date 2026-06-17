using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/StatusEffect")]
public class StatusEffectData : ScriptableObject
{
    [System.Serializable]
    public struct ModEntry
    {
        public StatType Stat;
        public float Value;
        public StatModifierType Type; // Add / Mult / MultTotal / Max / Min
    }

    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public float Duration { get; private set; } // 0 = permanent (talent)
    [field: SerializeField] public List<ModEntry> Modifiers { get; private set; } = new();
}
