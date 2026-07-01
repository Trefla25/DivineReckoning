using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/AbilityData")]
public class AbilityData : ScriptableObject
{
    public string label;
    public Sprite icon;

    [Header("Cast")]
    [Tooltip("Animator state to Play() while casting (e.g. \"Attack\"). Blank = none.")]
    public string animationState;
    [Range(0f, 4f)] public float castTime = 0.5f;
    [Tooltip("Root the player in place for the duration of the cast. " +
             "Off = the player can keep moving while this ability fires.")]
    public bool lockMovement = true;
    [Tooltip("Holds the cast animation after the effect fires so Idle/Walk doesn't stomp it. " +
               "Required for instant (castTime 0) abilities.")]
    [Range(0f, 2f)] public float recoveryTime = 0.4f;
    public float cooldown = 5f;
    public float resourceCost = 20f;
    [Tooltip("Reach in world units. Leave < 0 to default to the caster's auto-attack range (AttackRange stat).")]
    public float abilityRange = -1f;

    [Header("Targeting")]
    [SerializeReference] public TargetingStrategy targeting = new SelfTargeting();

    [Header("Effects")]
    [SerializeReference] public List<AbilityEffect> effects = new();

    [Header("VFX")]
    [Tooltip("Spawned on each target when effects resolve. Use the basic-attack hit for now; swap per-ability later.")]
    public ParticleSystem hitVfx;

    void OnEnable()
    {
        if (string.IsNullOrEmpty(label)) label = name;
        effects ??= new List<AbilityEffect>();
    }

    // Called by a targeting strategy / projectile once it has a concrete target.
    public void ApplyEffects(GameObject caster, GameObject target)
    {
        if (target == null) return;

        if (hitVfx != null)
            Instantiate(hitVfx, target.transform.position + Vector3.up, Quaternion.identity);

        foreach (var effect in effects)
            effect.Execute(caster, target);
    }

    public float ResolveRange(float autoAttackRange) => abilityRange < 0f ? autoAttackRange : abilityRange;
}