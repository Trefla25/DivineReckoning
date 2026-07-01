using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTargetingUtils : MonoBehaviour
{
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    [SerializeField] LayerMask enemyLayers;

    // Camera.main does a tagged-object lookup each call; this runs on every click and
    // ~10x/sec while a button is held, so cache it (lazily, in case it spawns late).
    Camera cam;
    Camera Cam => cam != null ? cam : (cam = Camera.main);

    public bool RaycastClick(out RaycastHit hit)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return Physics.Raycast(Cam.ScreenPointToRay(mousePosition), out hit, 100, clickableLayers);
    }

    public bool RaycastEnemy(out RaycastHit hit)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return Physics.Raycast(Cam.ScreenPointToRay(mousePosition), out hit, 100, enemyLayers);
    }

    // Reused so attack-move scans never allocate (called every hold-repath).
    static readonly Collider[] overlapBuffer = new Collider[32];

    /// <summary>
    /// Attack-move acquisition. <paramref name="searchCenter"/> (the click point) plus
    /// <paramref name="radius"/> gate WHICH enemies are candidates ("near the cursor"); among
    /// those we engage the one closest to the CURSOR — not the player. So the player will walk
    /// past nearer enemies to reach the one under/next-to the crosshair. This matches Shape of
    /// Dreams' "attack move on cursor" priority.
    /// </summary>
    public bool FindEnemyNear(Vector3 searchCenter, float radius, out InteractableUtils enemy)
    {
        enemy = null;
        int count = Physics.OverlapSphereNonAlloc(searchCenter, radius, overlapBuffer, enemyLayers, QueryTriggerInteraction.Collide);

        float bestSqr = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            if (!overlapBuffer[i].TryGetComponent(out InteractableUtils candidate))
                candidate = overlapBuffer[i].GetComponentInParent<InteractableUtils>();

            if (candidate == null || candidate.interactableType != InteractableType.Enemy) continue;
            if (candidate.stats == null || candidate.stats.IsDead) continue;

            float sqr = (candidate.transform.position - searchCenter).sqrMagnitude;   // closest to CURSOR
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                enemy = candidate;
            }
        }
        return enemy != null;
    }

    public void SpawnClickEffect(Vector3 point)
    {
        if (clickEffect != null)
            Instantiate(clickEffect, point + new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
    }
}
