using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    enum HitMode { StopOnFirst, Pierce }

    [SerializeField] float speed = 14f;
    [SerializeField] float lifetime = 5f;
    [Tooltip("Only these layers get hit. Set to your Enemy layer.")]
    [SerializeField] LayerMask hitMask = ~0;

    [Header("On hit")]
    [SerializeField] HitMode mode = HitMode.StopOnFirst;
    [Tooltip("Pierce only: max enemies to pass through. 0 = unlimited.")]
    [SerializeField] int maxPierce = 0;
    [Tooltip("On impact, damage every enemy inside the projectile's box collider, not just the struck one.")]
    [SerializeField] bool hitAllInBox = false;

    [Header("Grow")]
    [Tooltip("Hitbox starts at this fraction of its authored size and grows to full over Grow Time. 1 = no growth.")]
    [SerializeField, Range(0f, 1f)] float startSize = 1f;
    [Tooltip("Seconds to reach full size. 0 = no growth.")]
    [SerializeField] float growTime = 0f;
    [Tooltip("Growth shape over normalized time (0->1). Ease it to match the VFX.")]
    [SerializeField] AnimationCurve growCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    BoxCollider box;
    GameObject caster;
    AbilityData ability;
    readonly HashSet<GameObject> hitTargets = new();
    int pierced;
    bool finished;
    Vector3 baseBoxSize;
    float age;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        if (box != null) baseBoxSize = box.size;
    }

    public void Initialize(GameObject caster, AbilityData ability)
    {
        this.caster = caster;
        this.ability = ability;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (growTime > 0f && box != null)
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / growTime);
            float k = Mathf.Lerp(startSize, 1f, growCurve.Evaluate(t));
            box.size = baseBoxSize * k;
        }
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished) return;              // Destroy() is deferred; ignore extra same-frame hits
        if (other.gameObject == caster) return;
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;
        if (!other.TryGetComponent(out StatController stats) || stats.IsDead) return;
        if (hitTargets.Contains(stats.gameObject)) return;

        Damage(stats.gameObject);          // struck enemy always takes the hit
        if (hitAllInBox) DamageBox();      // plus everyone else inside the slash box

        if (mode == HitMode.StopOnFirst || (maxPierce > 0 && ++pierced >= maxPierce))
        {
            finished = true;
            Destroy(gameObject);
        }
    }

    void Damage(GameObject target)
    {
        if (!hitTargets.Add(target)) return;   // never double-hit the same enemy
        ability.ApplyEffects(caster, target);
    }

    void DamageBox()
    {
        if (box == null) return;
        Vector3 center = transform.TransformPoint(box.center);
        Vector3 half = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
        foreach (var col in Physics.OverlapBox(center, half, transform.rotation, hitMask))
            if (col.TryGetComponent(out StatController s) && !s.IsDead)
                Damage(s.gameObject);
    }
}
