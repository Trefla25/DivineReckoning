using UnityEngine;

[RequireComponent(typeof(StatController))]
public class ResourceRegen : MonoBehaviour
{
    [SerializeField] RegenMode mode = RegenMode.Passive;
    [SerializeField] float combatMemory = 3f;   // seconds counted as "in combat" after a hit

    StatController stats;
    float lastCombatTime = -999f;

    void Awake() => stats = GetComponent<StatController>();

    void Start()
    {
        // Rage-style pools start empty; passive pools start full (StatResource default).
        if (mode == RegenMode.DecayOutOfCombat)
        {
            stats.Stats.Resource.Set(0);
        }
    }

    bool InCombat => Time.time - lastCombatTime < combatMemory;

    // Refresh the combat timer; optionally generate resource (e.g. rage from a basic attack).
    public void RegisterCombat(float resourceGain = 0f)
    {
        lastCombatTime = Time.time;
        if (resourceGain > 0f)
        {
            stats.GainResource(resourceGain);
        }
    }

    void Update()
    {
        float rate = stats.GetValue(StatType.ResourceRegen);   // a Stat → buffs/items apply
        float delta = mode switch
        {
            RegenMode.Passive => rate,                    // mana / energy tick up
            RegenMode.DecayOutOfCombat => InCombat ? 0f : -rate,   // rage drains when idle
            _ => 0f,
        };

        if (delta != 0f) stats.Stats.Resource.Modify(delta * Time.deltaTime);
    }
}
