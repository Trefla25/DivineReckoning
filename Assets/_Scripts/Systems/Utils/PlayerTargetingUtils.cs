using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTargetingUtils : MonoBehaviour
{
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    [SerializeField] LayerMask enemyLayers;

    public bool RaycastClick(out RaycastHit hit)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out hit, 100, clickableLayers);
    }

    public bool RaycastEnemy(out RaycastHit hit)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out hit, 100, enemyLayers);
    }

    public void SpawnClickEffect(Vector3 point)
    {
        if (clickEffect != null)
            Instantiate(clickEffect, point + new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
    }
}
