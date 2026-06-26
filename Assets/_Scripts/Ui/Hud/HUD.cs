using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] EnergyBar healthBar;
    [SerializeField] EnergyBar resourceBar;

    [SerializeField] float maxHealth = 100f;
    [SerializeField] float maxResource = 100f;

    private void Start()
    {
        healthBar.SetMaxPoints(maxHealth);
        healthBar.SetPoints(maxHealth);

        resourceBar.SetMaxPoints(maxResource);
        resourceBar.SetPoints(maxResource);
    }
}
