using UnityEngine;

public class PlayerAnimatorUtils : MonoBehaviour
{
    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string ATTACK = "Attack";
    const string PICKUP = "Pickup";
    const string UPPER_LAYER = "UpperBody";
    const string ATTACK_SPEED_PARAM = "AttackSpeed";

    [SerializeField] float upperBlendSpeed = 12f;

    Animator animator;
    int upperLayer;
    string baseState;
    float upperTarget;
    int upperStateHash;
    bool upperStarted;

    void Awake()
    {
        animator = GetComponent<Animator>();
        upperLayer = animator.GetLayerIndex(UPPER_LAYER);
    }

    void Update()
    {
        float weight = animator.GetLayerWeight(upperLayer);
        weight = Mathf.MoveTowards(weight, upperTarget, upperBlendSpeed * Time.deltaTime);
        animator.SetLayerWeight(upperLayer, weight);

        if (upperStateHash != 0 && !animator.IsInTransition(upperLayer))
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(upperLayer);
            if (info.shortNameHash == upperStateHash)
            {
                if (info.normalizedTime < 1f) upperStarted = true;
                else if (upperStarted) { upperTarget = 0f; upperStateHash = 0; }
            }
        }
    }

    public void PlayIdle() => PlayBase(IDLE, fade: 0.12f);
    public void PlayWalk() => PlayBase(WALK, fade: 0.12f);

    void PlayBase(string state, float fade)
    {
        if (state == baseState) return;
        baseState = state;
        animator.CrossFadeInFixedTime(state, fade, 0);
    }

    public void PlayAttack(float speedMultiplier)
    {
        animator.SetFloat(ATTACK_SPEED_PARAM, speedMultiplier);
        PlayUpper(ATTACK);
    }

    public void PlayAbility(string state)
    {
        if (string.IsNullOrEmpty(state)) return;
        animator.SetFloat(ATTACK_SPEED_PARAM, 1f);
        PlayUpper(state);
    }

    public void PlayPickup() => PlayUpper(PICKUP);

    void PlayUpper(string state)
    {
        upperTarget = 1f;
        upperStateHash = Animator.StringToHash(state);
        upperStarted = false;
        animator.CrossFadeInFixedTime(state, 0.02f, upperLayer, 0f);
    }
}
