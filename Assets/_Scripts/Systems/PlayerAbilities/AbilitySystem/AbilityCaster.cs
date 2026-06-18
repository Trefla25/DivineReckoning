using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(StatController), typeof(PlayerTargetingUtils), typeof(PlayerAnimatorUtils))]
public class AbilityCaster : MonoBehaviour
{
    [SerializeField] AbilityData[] hotbar = new AbilityData[4];
    [SerializeField] Key[] keys = { Key.Q, Key.W, Key.E, Key.R };

    public StatController Stats { get; private set; }
    public PlayerTargetingUtils Targeting { get; private set; }
    public StatController SelectedTarget => controller != null ? controller.SelectedTarget : null;
    public float AttackRange => Stats.GetValue(StatType.AttackRange);

    PlayerAnimatorUtils animator;
    PlayerController controller;   // optional; null for non-player casters
    PlayerMovementUtils movement;

    float[] cooldownEnds;
    bool casting;

    void Awake()
    {
        Stats = GetComponent<StatController>();
        Targeting = GetComponent<PlayerTargetingUtils>();
        animator = GetComponent<PlayerAnimatorUtils>();
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovementUtils>();
        cooldownEnds = new float[hotbar.Length];
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        for (int i = 0; i < hotbar.Length && i < keys.Length; i++)
            if (Keyboard.current[keys[i]].wasPressedThisFrame) TryCast(i);
    }

    void TryCast(int slot)
    {
        AbilityData ability = hotbar[slot];
        if (ability == null || casting) return;
        if (Time.time < cooldownEnds[slot]) return;                      // on cooldown
        if (ability.targeting != null && !ability.targeting.Validate(this, ability)) return; // e.g. no target
        if (!Stats.HasResource(ability.resourceCost)) return;           // not enough resource
        StartCoroutine(CastRoutine(slot, ability));
    }

    IEnumerator CastRoutine(int slot, AbilityData ability)
    {
        casting = true;
        controller?.SetBusy(true);
        cooldownEnds[slot] = Time.time + ability.cooldown;

        FaceCastDirection(ability);
        animator.PlayAbility(ability.animationState);

        if (ability.castTime > 0f) yield return new WaitForSeconds(ability.castTime);

        Stats.SpendResource(ability.resourceCost);
        ability.targeting?.Execute(this, ability);

        // Keep the animation up so SetAnimations() doesn't override it the same frame (instant casts).
        if (ability.recoveryTime > 0f) yield return new WaitForSeconds(ability.recoveryTime);

        controller?.SetBusy(false);
        casting = false;
    }

    void FaceCastDirection(AbilityData ability)
    {
        if (movement == null || ability.targeting is SelfTargeting) return;

        if (SelectedTarget != null)
        {
            // targeted (Strike) → face the enemy
            movement.FaceTowards(SelectedTarget.transform.position);
        }
        else if (Targeting.RaycastClick(out RaycastHit hit))
        { 
            // skillshot/ground → face the cursor
            movement.FaceTowards(hit.point);
        }
    }

    // --- HUD hooks ---
    public AbilityData GetAbility(int slot) => hotbar[slot];
    public float GetCooldownNormalized(int slot)   // 1 = just used, 0 = ready
    {
        AbilityData a = hotbar[slot];
        if (a == null || a.cooldown <= 0f) return 0f;
        return Mathf.Clamp01((cooldownEnds[slot] - Time.time) / a.cooldown);
    }
}
