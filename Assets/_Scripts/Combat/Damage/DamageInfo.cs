using UnityEngine;

public struct DamageInfo
{
    public float Power;
    public float ArmorPen;   // 0..1 fraction of defense ignored
    public GameObject Source;
    public DamageInfo(float power, float armorPen, GameObject source)
    { Power = power; ArmorPen = armorPen; Source = source; }
}
