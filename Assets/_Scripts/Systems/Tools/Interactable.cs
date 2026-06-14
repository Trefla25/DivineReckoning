using UnityEngine;

public class Interactable : MonoBehaviour
{
    public DealDamageActor damageActor { get; private set; }
    public InteractableType interactableType;

    void Awake()
    {
        if(interactableType == InteractableType.Enemy)
        {
            damageActor = GetComponent<DealDamageActor>();
        }
    }

    public void InteractWithItem()
    {
        Destroy(gameObject);
    }
}
