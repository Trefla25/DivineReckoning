using System;
using UnityEngine;

public class StatController : MonoBehaviour
{
    [SerializeField] private ClassData classData;

    public StatSheet Stats { get; private set; }
    public bool IsDead { get; private set; }
    public event Action<StatController>? OnDied;

    void Awake() => Stats = new StatSheet(classData.BaseStats.ToPairs());

    public float GetValue(StatType type) => Stats.Value(type);

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        Stats.Health.Modify(-Mathf.Max(0, amount));
        if (Stats.Health.IsEmpty) Die();
    }

    public void Heal(float amount) => Stats.Health.Modify(Mathf.Max(0, amount));

    private void Die()
    {
        IsDead = true;
        OnDied?.Invoke(this);
        Destroy(gameObject);
    }

    public bool HasResource(float amount) => Stats.Resource.Current >= amount;
    public void SpendResource(float amount) => Stats.Resource.Modify(-Mathf.Max(0, amount));
    public void GainResource(float amount) => Stats.Resource.Modify(Mathf.Max(0, amount));
}
