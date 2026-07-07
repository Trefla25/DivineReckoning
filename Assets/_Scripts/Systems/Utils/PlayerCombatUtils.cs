using UnityEngine;

[RequireComponent(typeof(StatController))]
public class PlayerCombatUtils : MonoBehaviour
{
    [Header("Feel (not stats)")]
    [Tooltip("Length (seconds) of the Attack animation clip at 1x speed. " +
             "Used to time-stretch the clip so it plays fully within each attack interval.")]
    [SerializeField] float attackAnimLength = 1f;
    [Range(0f, 1f)]
    [Tooltip("When along the swing the hit lands (damage + VFX). " +
         "1 = at the end of the animation, 0.5 = halfway, 0 = at the start.")]
    [SerializeField] float animationFireDamage = 1f;
    [SerializeField] ParticleSystem attackEffect;

    StatController stats;
    void Awake() => stats = GetComponent<StatController>();

    // AttackSpeed stat = attacks/sec; the controller wants seconds-per-attack.
    public float AttackSpeed => 1f / Mathf.Max(0.01f, stats.GetValue(StatType.AttackSpeed));
    public float AttackAnimLength => attackAnimLength;
    public float AnimationFireDamage => animationFireDamage;
    public float AttackRange => stats.GetValue(StatType.AttackRange);

    public void DealDamage(InteractableUtils target)
    {
        if (target.stats == null) return;
        Instantiate(attackEffect, target.transform.position + Vector3.up, Quaternion.identity);

        var info = new DamageInfo(
            power: stats.GetValue(StatType.Power),
            armorPen: stats.GetValue(StatType.ArmorPen),
            source: this);

        DamageCalculator.Apply(info, target.stats);

        if (TryGetComponent(out ResourceRegen regen))
        {
            regen.RegisterCombat(stats.GetValue(StatType.ResourceGainOnHit));
        }
    }
}
