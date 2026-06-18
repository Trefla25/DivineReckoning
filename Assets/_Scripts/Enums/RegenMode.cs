using UnityEngine;

public enum RegenMode
{
    Passive,            // regenerates every second (mana, energy)
    DecayOutOfCombat,   // builds from combat actions, drains when idle (rage)
    None,               // pure spend pool, no automatic regen
}