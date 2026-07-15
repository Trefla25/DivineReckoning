using UnityEngine;

public class HudBillboard : MonoBehaviour
{
    public Transform cam;
    public Transform anchor;
    public float heightOffset = 0.3f;

    void Awake()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        if (anchor != null)
            transform.position = anchor.position + Vector3.up * heightOffset;
        transform.LookAt(transform.position + cam.forward);
    }
}
