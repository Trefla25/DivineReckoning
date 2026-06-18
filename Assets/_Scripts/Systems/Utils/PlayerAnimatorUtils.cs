using UnityEngine;

public class PlayerAnimatorUtils : MonoBehaviour
{
    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string ATTACK = "Attack";
    const string PICKUP = "Pickup";

    Animator animator;
    void Awake() => animator = GetComponent<Animator>();

    public void PlayIdle() => animator.Play(IDLE);
    public void PlayWalk() => animator.Play(WALK);
    public void PlayAttack() => animator.Play(ATTACK);
    public void PlayPickup() => animator.Play(PICKUP);
    public void PlayAbility(string state)
    {
        if (!string.IsNullOrEmpty(state))
        {
            animator.Play(state);
        }
    }
}
