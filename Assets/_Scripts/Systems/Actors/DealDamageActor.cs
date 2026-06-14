using UnityEngine;

public class DealDamageActor : MonoBehaviour
{
    public int maxHealth;

    public int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        // Handle death logic here (e.g., play animation, drop loot, etc.)
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
