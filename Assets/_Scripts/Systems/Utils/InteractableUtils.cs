using UnityEngine;

public class InteractableUtils : MonoBehaviour
{
    public StatController stats { get; private set; }
    public InteractableType interactableType;

    void Awake()
    {
        if (interactableType == InteractableType.Enemy)
            stats = GetComponent<StatController>();
    }

    public void InteractWithItem() => Destroy(gameObject);
}
