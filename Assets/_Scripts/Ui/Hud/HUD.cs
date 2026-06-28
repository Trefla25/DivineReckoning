using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] StatController stats;
    [SerializeField] EnergyBar healthBar;
    [SerializeField] EnergyBar resourceBar;

    private void Start()
    {
        var health = stats.Stats.Health;
        var resource = stats.Stats.Resource;

        health.OnChanged += healthBar.SetValues;
        resource.OnChanged += resourceBar.SetValues;

        healthBar.SetValues(health.Current, health.Max);
        resourceBar.SetValues(resource.Current, resource.Max);
    }

    private void OnDestroy()
    {
        if (stats == null || stats.Stats == null) return;

        stats.Stats.Health.OnChanged -= healthBar.SetValues;
        stats.Stats.Resource.OnChanged -= resourceBar.SetValues;
    }
}
