using System;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    [SerializeField] float speed = 14f;
    [SerializeField] float lifetime = 5f;

    GameObject caster;
    AbilityData ability;
    bool consumed;

    public void Initialize(GameObject caster, AbilityData ability)
    {
        this.caster = caster;
        this.ability = ability;
        Destroy(gameObject, lifetime);   // safety cleanup if it never hits
    }

    void Update() => transform.position += transform.forward * (speed * Time.deltaTime);

    void OnTriggerEnter(Collider other)
    {
        if (consumed || other.gameObject == caster) return;
        if (!other.TryGetComponent(out StatController _)) return;

        consumed = true;
        ability.ApplyEffects(caster, other.gameObject);
        Destroy(gameObject);
    }
}
