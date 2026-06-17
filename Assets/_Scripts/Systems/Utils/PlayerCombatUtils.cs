using UnityEngine;

[RequireComponent(typeof(StatController))]
public class PlayerCombatUtils : MonoBehaviour
{
    [Header("Feel (not stats)")]
    [SerializeField] float attackDelay = 0.3f;
    [SerializeField] float baseAttackRange = 1.5f;
    [SerializeField] ParticleSystem attackEffect;

    StatController stats;
    void Awake() => stats = GetComponent<StatController>();

    // AttackSpeed stat = attacks/sec; the controller wants seconds-per-attack.
    public float AttackSpeed => 1f / Mathf.Max(0.01f, stats.GetValue(StatType.AttackSpeed));
    public float AttackDelay => attackDelay;
    public float AttackRange => baseAttackRange;

    public void DealDamage(InteractableUtils target)
    {
        if (target.stats == null) return;
        Instantiate(attackEffect, target.transform.position + Vector3.up, Quaternion.identity);

        var info = new DamageInfo(
            power: stats.GetValue(StatType.Power),
            armorPen: stats.GetValue(StatType.ArmorPen),
            source: this);

        DamageCalculator.Apply(info, target.stats);
    }
}
