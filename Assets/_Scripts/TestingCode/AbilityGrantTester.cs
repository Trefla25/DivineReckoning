using UnityEngine;

public class AbilityGrantTester : MonoBehaviour
{
    [SerializeField] AbilityCaster caster;
    [SerializeField] AbilityData ability;

    public void Grant()
    {
        if (!caster.AddAbility(ability))
            Debug.Log("Action bar is full.");
    }
}
