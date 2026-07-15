using UnityEngine;

public class HealthBarBinder : MonoBehaviour
{
    [SerializeField] StatController stats;
    [SerializeField] EnergyBar healthBar;
    [SerializeField] GameObject barRoot;
    [SerializeField] float hideDelay = 3f;

    float lastDamageTime = -1f;
    float lastHealth;

    void Awake()
    {
        if (stats == null) stats = GetComponentInParent<StatController>();
    }

    void Start()
    {
        var health = stats.Stats.Health;
        lastHealth = health.Current;
        health.OnChanged += OnHealthChanged;
        healthBar.SetValues(health.Current, health.Max);
        if (barRoot != null) barRoot.SetActive(false);

        Debug.Log($"[HealthBarBinder] START on '{name}' bound to StatController id={stats.GetInstanceID()}", this);
    }

    void OnHealthChanged(float current, float max)
    {
        Debug.Log($"[HealthBarBinder] CHANGED on '{name}' {current}/{max} (last {lastHealth})", this);

        healthBar.SetValues(current, max);
        if (current < lastHealth)
        {
            lastDamageTime = Time.time;
            if (barRoot != null) barRoot.SetActive(true);
        }
        lastHealth = current;
    }

    void Update()
    {
        if (barRoot == null || !barRoot.activeSelf) return;

        if (Time.time >= lastDamageTime + hideDelay)
            barRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (stats != null && stats.Stats != null)
            stats.Stats.Health.OnChanged -= OnHealthChanged;
    }
}
