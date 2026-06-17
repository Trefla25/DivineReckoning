using UnityEngine;

[CreateAssetMenu(menuName = "Data/Class")]
public class ClassData : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public StatBlock BaseStats { get; private set; }
    // Later: starting abilities, available specs, etc.
}
