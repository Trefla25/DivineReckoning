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

    public void PlayIdle() => Play(IDLE);
    public void PlayWalk() => Play(WALK);
    public void PlayAttack() => Play(ATTACK, forceRestart: true);
    public void PlayPickup() => Play(PICKUP, forceRestart: true);

    public void PlayAbility(string state)
    {
        if (!string.IsNullOrEmpty(state)) Play(state, forceRestart: true);
    }

    void Play(string state, bool forceRestart = false)
    {
        if (!forceRestart && state == currentState) return;

        currentState = state;
        animator.Play(state);
    }
}
