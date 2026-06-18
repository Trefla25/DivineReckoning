using UnityEngine;

[RequireComponent(typeof(PlayerMovementUtils), typeof(PlayerTargetingUtils), typeof(PlayerCombatUtils))]
[RequireComponent(typeof(PlayerInputUtils), typeof(PlayerAnimatorUtils))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float holdRepathInterval = 0.1f;

    public StatController SelectedTarget { get; private set; }

    PlayerInputUtils playerInput;
    PlayerMovementUtils playerMovement;
    PlayerTargetingUtils playerTargeting;
    PlayerCombatUtils playerCombat;
    PlayerAnimatorUtils playerAnimator;
    InteractableUtils currentTarget;

    bool playerBusy;
    float nextRepathTime;

    public bool IsBusy => playerBusy;

    void Awake()
    {
        playerInput = GetComponent<PlayerInputUtils>();
        playerMovement = GetComponent<PlayerMovementUtils>();
        playerTargeting = GetComponent<PlayerTargetingUtils>();
        playerCombat = GetComponent<PlayerCombatUtils>();
        playerAnimator = GetComponent<PlayerAnimatorUtils>();
    }

    void OnEnable()
    {
        playerInput.MoveClick += OnMoveClick;
        playerInput.SelectClick += OnSelectClick;
    }
    void OnDisable()
    {
        playerInput.MoveClick -= OnMoveClick;
        playerInput.SelectClick -= OnSelectClick;
    }

    void Update()
    {
        FollowTarget();
        HoldToMove();
        playerMovement.FaceMoveDirection();
        SetAnimations();
    }

    public void SetBusy(bool value)
    {
        playerBusy = value;
        if (!value) 
        {
            SetAnimations();
        }
    }

    void OnMoveClick(bool held)
    {
        nextRepathTime = Time.time + holdRepathInterval;

        if (!playerTargeting.RaycastClick(out RaycastHit hit)) return;

        if (hit.transform.TryGetComponent(out InteractableUtils interactable))
        {
            currentTarget = interactable;
            if (interactable.interactableType == InteractableType.Enemy)
                SelectTarget(interactable.stats);
            if (!held)
                playerTargeting.SpawnClickEffect(hit.point);
        }
        else
        {
            currentTarget = null;
            playerMovement.MoveTo(hit.point);
            if (!held)
                playerTargeting.SpawnClickEffect(hit.point);
        }
    }

    void OnSelectClick()
    {
        if (!playerTargeting.RaycastClick(out RaycastHit hit)) return;

        if (hit.transform.TryGetComponent(out InteractableUtils interactable)
            && interactable.interactableType == InteractableType.Enemy)
        {
            SelectTarget(interactable.stats);
        }
    }

    void HoldToMove()
    {
        if (playerInput.IsHeld && Time.time >= nextRepathTime)
        {
            OnMoveClick(true);
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

        if (currentTarget.interactableType == InteractableType.Enemy)
            playerMovement.FaceTowards(currentTarget.transform.position);

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

    void SelectTarget(StatController target)
    {
        if (SelectedTarget != null)
        {
            SelectedTarget.OnDied -= OnSelectedDied;
        }

        SelectedTarget = target;

        if (SelectedTarget != null)
        {
            SelectedTarget.OnDied += OnSelectedDied;
        }

        Debug.Log("Target selected: " + target.name);
    }

    void OnSelectedDied(StatController s) => SelectedTarget = null;

    void SendAttack()
    {
        if (currentTarget == null) return;

        if (currentTarget.stats == null || currentTarget.stats.IsDead)
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

        if (playerMovement.IsStopped)
        {
            playerAnimator.PlayIdle();
        }
        else
        {
            playerAnimator.PlayWalk();
        }
    }
}
