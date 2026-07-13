using UnityEngine;

public class HudBillboard : MonoBehaviour
{
    public Transform cam;
    public Transform anchor;          // assigned per prefab
    public float heightOffset = 0.3f; // constant above the anchor

    void LateUpdate()
    {
        if (anchor != null)
            transform.position = anchor.position + Vector3.up * heightOffset;
        transform.LookAt(transform.position + cam.forward);
    }
}
