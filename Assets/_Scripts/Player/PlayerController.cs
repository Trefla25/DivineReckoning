using UnityEngine;

[RequireComponent(typeof(PlayerMovementUtils), typeof(PlayerTargetingUtils), typeof(PlayerCombatUtils))]
[RequireComponent(typeof(PlayerInputUtils), typeof(PlayerAnimatorUtils))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float holdRepathInterval = 0.1f;

    PlayerInputUtils playerInput;
    PlayerMovementUtils playerMovement;
    PlayerTargetingUtils playerTargeting;
    PlayerCombatUtils playerCombat;
    PlayerAnimatorUtils playerAnimator;

    InteractableUtils currentTarget;
    bool playerBusy;
    float nextRepathTime;

    void Awake()
    {
        playerInput = GetComponent<PlayerInputUtils>();
        playerMovement = GetComponent<PlayerMovementUtils>();
        playerTargeting = GetComponent<PlayerTargetingUtils>();
        playerCombat = GetComponent<PlayerCombatUtils>();
        playerAnimator = GetComponent<PlayerAnimatorUtils>();
    }

    void OnEnable() => playerInput.MoveClick += ClickToMove;
    void OnDisable() => playerInput.MoveClick -= ClickToMove;

    void Update()
    {
        FollowTarget();
        HoldToMove();
        playerMovement.FaceMoveDirection();
        SetAnimations();
    }

    void ClickToMove(bool held)
    {
        nextRepathTime = Time.time + holdRepathInterval;

        if (!playerTargeting.RaycastClick(out RaycastHit hit)) return;

        if (hit.transform.CompareTag("Interactable"))
        {
            currentTarget = hit.transform.GetComponent<InteractableUtils>();
            if (!held) playerTargeting.SpawnClickEffect(hit.point);
        }
        else
        {
            currentTarget = null;
            playerMovement.MoveTo(hit.point);
            if (!held) playerTargeting.SpawnClickEffect(hit.point);
        }
    }

    void HoldToMove()
    {
        if (playerInput.IsHeld && Time.time >= nextRepathTime)
        {
            ClickToMove(true);
        }
    }

    void FollowTarget()
    {
        if (currentTarget == null) return;

        if (Vector3.Distance(currentTarget.transform.position, transform.position) <= playerCombat.AttackRange)
        {
            ReachDistance();
        }
        else
        {
            playerMovement.MoveTo(currentTarget.transform.position);
        }

    }

    void ReachDistance()
    {
        playerMovement.Stop();

        if (playerBusy) return;
        playerBusy = true;

        switch (currentTarget.interactableType)
        {
            case InteractableType.Enemy:
                playerAnimator.PlayAttack();
                Invoke(nameof(SendAttack), playerCombat.AttackDelay);
                Invoke(nameof(ResetBusyState), playerCombat.AttackSpeed);
                break;
            case InteractableType.Item:
                playerAnimator.PlayPickup();
                currentTarget.InteractWithItem();
                currentTarget = null;
                Invoke(nameof(ResetBusyState), 0.5f);
                break;
        }
    }

    void SendAttack()
    {
        if (currentTarget == null) return;

        if (currentTarget.damageActor.currentHealth <= 0)
        {
            currentTarget = null;
            return;
        }

        playerCombat.DealDamage(currentTarget);
    }

    void ResetBusyState()
    {
        playerBusy = false;
        SetAnimations();
    }

    void SetAnimations()
    {
        if (playerBusy) return;

        if (playerMovement.IsStopped) playerAnimator.PlayIdle();
        else playerAnimator.PlayWalk();
    }
}
