using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] StatController stats;
    [SerializeField] EnergyBar[] healthBars;
    [SerializeField] EnergyBar[] resourceBars;

    private void Start()
    {
        var health = stats.Stats.Health;
        var resource = stats.Stats.Resource;

        foreach (var bar in healthBars)
        {
            health.OnChanged += bar.SetValues;
            bar.SetValues(health.Current, health.Max);
        }

        foreach (var bar in resourceBars)
        {
            resource.OnChanged += bar.SetValues;
            bar.SetValues(resource.Current, resource.Max);
        }
    }

    private void OnDestroy()
    {
        if (stats == null || stats.Stats == null) return;

        foreach (var bar in healthBars)
            stats.Stats.Health.OnChanged -= bar.SetValues;

        foreach (var bar in resourceBars)
            stats.Stats.Resource.OnChanged -= bar.SetValues;
    }
}
