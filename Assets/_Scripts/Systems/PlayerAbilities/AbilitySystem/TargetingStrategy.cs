using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class TargetingStrategy
{
    // Checked before the cast begins. Return false to block the cast (no cooldown/resource spent).
    public virtual bool Validate(AbilityCaster caster, AbilityData ability) => true;

    // Deliver the ability's effects. Synchronous for self/single/AOE; deferred for projectile.
    public abstract void Execute(AbilityCaster caster, AbilityData ability);
}

[Serializable]
public class SelfTargeting : TargetingStrategy
{
    public override void Execute(AbilityCaster caster, AbilityData ability)
        => ability.ApplyEffects(caster.gameObject, caster.gameObject);
}

[Serializable]
public class SelectedTargeting : TargetingStrategy   // WoW-style: hits your selected enemy
{
    public override bool Validate(AbilityCaster caster, AbilityData ability)
    {
        StatController target = caster.SelectedTarget;
        if (target == null || target.IsDead) return false;

        float range = ability.ResolveRange(caster.AttackRange);
        return Vector3.Distance(caster.transform.position, target.transform.position) <= range;
    }

    public override void Execute(AbilityCaster caster, AbilityData ability)
    {
        StatController target = caster.SelectedTarget;
        if (target == null || target.IsDead) return;
        ability.ApplyEffects(caster.gameObject, target.gameObject);
    }
}

[Serializable]
public class MouseOverTargeting : TargetingStrategy   // cursor / mouseover cast
{
    public override bool Validate(AbilityCaster caster, AbilityData ability)
        => caster.Targeting.RaycastEnemy(out RaycastHit hit)
           && hit.collider.TryGetComponent(out StatController target)
           && !target.IsDead;

    public override void Execute(AbilityCaster caster, AbilityData ability)
    {
        if (caster.Targeting.RaycastEnemy(out RaycastHit hit) &&
            hit.collider.TryGetComponent(out StatController _))
        {
            ability.ApplyEffects(caster.gameObject, hit.collider.gameObject);
        }
    }
}

[Serializable]
public class GroundAOETargeting : TargetingStrategy
{
    public float radius = 4f;
    public LayerMask targetMask = ~0;
    public GameObject aoeVfx;

    public override void Execute(AbilityCaster caster, AbilityData ability)
    {
        if (!caster.HasAimPoint) return;
        Vector3 point = caster.AimPoint;

        if (aoeVfx != null)
            UnityEngine.Object.Instantiate(aoeVfx, point, Quaternion.identity);

        // Dedupe: one object can have multiple colliders.
        var seen = new HashSet<GameObject>();
        foreach (var col in Physics.OverlapSphere(point, radius, targetMask))
        {
            if (!col.TryGetComponent(out StatController _)) continue;
            if (seen.Add(col.gameObject))
                ability.ApplyEffects(caster.gameObject, col.gameObject);
        }
    }
}

[Serializable]
public class ProjectileTargeting : TargetingStrategy
{
    public ProjectileMove projectilePrefab;

    public override void Execute(AbilityCaster caster, AbilityData ability)
    {
        if (projectilePrefab == null) return;

        // Aim flat toward the point captured at cast start; fall back to facing direction.
        Vector3 origin = caster.transform.position + Vector3.up;
        Vector3 dir = caster.transform.forward;
        if (caster.HasAimPoint)
        {
            Vector3 flat = caster.AimPoint - origin; flat.y = 0;
            if (flat.sqrMagnitude > 0.01f) dir = flat.normalized;
        }

        var projectile = UnityEngine.Object.Instantiate(
            projectilePrefab, origin, Quaternion.LookRotation(dir));
        projectile.Initialize(caster.gameObject, ability);
    }
}
