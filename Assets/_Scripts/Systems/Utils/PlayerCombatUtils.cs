using UnityEngine;

public class PlayerCombatUtils : MonoBehaviour
{
    [Header("BasicAttack")]
    [SerializeField] float attackSpeed = 1.5f;
    [SerializeField] float attackDelay = 0.3f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] int attackDamage = 1;
    [SerializeField] ParticleSystem attackEffect;

    public float AttackSpeed => attackSpeed;
    public float AttackDelay => attackDelay;
    public float AttackRange => attackRange;

    public void DealDamage(InteractableUtils target)
    {
        Instantiate(attackEffect, target.transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
        target.GetComponent<DealDamageActor>().TakeDamage(attackDamage);
    }
}
