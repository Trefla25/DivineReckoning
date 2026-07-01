using UnityEngine;

public class PlayerAnimatorUtils : MonoBehaviour
{
    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string ATTACK = "Attack";
    const string PICKUP = "Pickup";

    Animator animator;
    string currentState;

    void Awake() => animator = GetComponent<Animator>();

    public void PlayIdle() => Play(IDLE, fade: 0.12f);
    public void PlayWalk() => Play(WALK, fade: 0.12f);

    // Scales playback so the whole attack clip plays in time with attack speed
    // (faster attacks => faster animation), instead of getting cut to a "bonk".
    public void PlayAttack(float speedMultiplier) => Play(ATTACK, forceRestart: true, speed: speedMultiplier, fade: 0.02f);
    public void PlayPickup() => Play(PICKUP, forceRestart: true, fade: 0.02f);

    public void PlayAbility(string state)
    {
        if (!string.IsNullOrEmpty(state)) Play(state, forceRestart: true, fade: 0.02f);
    }

    void Play(string state, bool forceRestart = false, float speed = 1f, float fade = 0.1f)
    {
        animator.speed = speed;   // keep speed in sync even when we early-out below

        if (!forceRestart && state == currentState) return;

        currentState = state;
        // CrossFade (not Play) blends between states so Idle<->Walk doesn't hard-cut/pop.
        // A transition also restarts the destination state from 0, so repeat attacks
        // re-animate instead of parking on the finished clip's last frame.
        animator.CrossFadeInFixedTime(state, fade);
    }
}
