using UnityEngine;

public struct DamageInfo
{
    public float Power;
    public float ArmorPen;   // 0..1 fraction of defense ignored
    public object Source;
    public DamageInfo(float power, float armorPen, object source)
    { Power = power; ArmorPen = armorPen; Source = source; }
}
