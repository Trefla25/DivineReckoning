using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    [SerializeField] AudioClip[] footstepClips;
    [SerializeField, Range(0f, 1f)] float footstepVolume = 0.5f;

    void OnFootstep(AnimationEvent animationEvent)
    {
        // Blend trees fire events from every active clip; only react to
        // the dominant one or idle->run blends play double footsteps.
        if (animationEvent.animatorClipInfo.weight < 0.5f) return;
        if (footstepClips.Length == 0) return;

        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        AudioSource.PlayClipAtPoint(clip, transform.position, footstepVolume);
    }

    void OnLand(AnimationEvent animationEvent) { }
}
