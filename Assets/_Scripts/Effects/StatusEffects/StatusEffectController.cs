using System.Collections;
using UnityEngine;

[RequireComponent(typeof(StatController))]
public class StatusEffectController : MonoBehaviour
{
    StatController stats;
    void Awake() => stats = GetComponent<StatController>();

    public void Apply(StatusEffectData data)
    {
        object source = data; // unique-per-asset; use a fresh token if you need multiple stacks

        foreach (var m in data.Modifiers)
            stats.Stats.AddModifier(m.Stat, new StatModifier<StatModifierData>(
                m.Value, new StatModifierData(m.Type, source)));

        if (data.Duration > 0)
            StartCoroutine(RemoveAfter(data.Duration, source));
    }

    IEnumerator RemoveAfter(float seconds, object source)
    {
        yield return new WaitForSeconds(seconds);
        stats.Stats.RemoveSourceEverywhere(source);
    }
}
