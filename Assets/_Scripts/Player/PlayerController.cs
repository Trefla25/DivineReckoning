using UnityEngine;

[RequireComponent(typeof(PlayerMovementUtils), typeof(PlayerTargetingUtils), typeof(PlayerCombatUtils))]
[RequireComponent(typeof(PlayerInputUtils), typeof(PlayerAnimatorUtils))]
public class PlayerController : MonoBehaviour
{
    [Header("Click cadence")]
    [Tooltip("How often a held mouse button re-issues its command (move / attack-move).")]
    [SerializeField] float holdRepathInterval = 0.1f;

    [Header("Attack-move (Shape of Dreams style)")]
    [Tooltip("Left-click acquires the living enemy closest to the CURSOR, considering enemies within " +
             "this radius of the click point. Bigger = more forgiving aim (SoD is generous). " +
             "No enemy within radius -> left-click just moves there, like right-click.")]
    [SerializeField] float acquireRadius = 8f;

    [Tooltip("Extra reach added on top of AttackRange for the kite fallback ONLY (step 3). " +
             "When the cursor scan finds no enemy, we still acquire the enemy nearest the player " +
             "within (AttackRange + this), so you don't have to be pixel-perfect in range. " +
             "Set to 0 to fall back to exactly AttackRange.")]
    [SerializeField] float kiteFallbackBonusRange = 1.5f;

    public StatController SelectedTarget { get; private set; }

    enum Command { None, Move, Attack }

    PlayerInputUtils playerInput;
    PlayerMovementUtils playerMovement;
    PlayerTargetingUtils playerTargeting;
    PlayerCombatUtils playerCombat;
    PlayerAnimatorUtils playerAnimator;

    Command command;
    InteractableUtils currentTarget;     // what we walk to / interact with

    bool playerBusy;                     // abilities: blocks autos & owns the animator
    bool movementLocked;                 // abilities that lock the player in place
    float nextRepathTime;
    float nextAttackTime;                // auto-attack cooldown gate
    float attackAnimUntil;               // keep the attack/pickup pose up this long

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
        playerInput.MoveClick += OnRightClick;
        playerInput.AttackMoveClick += OnLeftClick;
    }
    void OnDisable()
    {
        playerInput.MoveClick -= OnRightClick;
        playerInput.AttackMoveClick -= OnLeftClick;
    }

    void Update()
    {
        HandleHold();
        UpdateMovement();
        UpdateAutoAttack();
        UpdateTargetRetention();   // drop a target we've walked away from
        UpdateFacing();
        SetAnimations();
    }

    // --- input ----------------------------------------------------------------

    // RMB: movement only. Never attacks/selects enemies (that's left-click now).
    // Clicking an item still walks over to pick it up; clicking an enemy or the
    // ground just moves there (autos keep firing while you go, so you can kite).
    void OnRightClick(bool held)
    {
        nextRepathTime = Time.time + holdRepathInterval;
        if (!playerTargeting.RaycastClick(out RaycastHit hit)) return;

        if (hit.transform.TryGetComponent(out InteractableUtils interactable)
            && interactable.interactableType == InteractableType.Item)
        {
            SetAttackTarget(interactable);   // walk over & pick up
            if (!held) playerTargeting.SpawnClickEffect(hit.point);
        }
        else
        {
            IssueMove(hit.point, held);      // ground OR enemy -> just move there
        }
    }

    // LMB: attack-move. Acquisition priority:
    //   1. cursor directly on a living enemy            -> that enemy
    //   2. closest-to-CURSOR within acquireRadius        -> SoD "attack move on cursor"
    //   3. kite fallback: closest-to-PLAYER within AttackRange
    //   4. nothing in range                              -> just move there (like RMB)
    void OnLeftClick(bool held)
    {
        nextRepathTime = Time.time + holdRepathInterval;
        if (!playerTargeting.RaycastClick(out RaycastHit hit)) return;

        InteractableUtils enemy = null;
        bool onEnemy = hit.transform.TryGetComponent(out InteractableUtils hovered)
                       && hovered.interactableType == InteractableType.Enemy
                       && hovered.stats != null && !hovered.stats.IsDead;

        if (onEnemy)
            enemy = hovered;
        else if (!playerTargeting.FindEnemyNear(hit.point, acquireRadius, out enemy))
            playerTargeting.FindEnemyNear(transform.position, playerCombat.AttackRange + kiteFallbackBonusRange, out enemy);   // kite fallback (+ forgiving bonus)

        if (enemy != null)
            SetAttackTarget(enemy);
        else
            IssueMove(hit.point, held);
    }

    void HandleHold()
    {
        if (Time.time < nextRepathTime) return;

        if (playerInput.IsMoveHeld) OnRightClick(true);
        else if (playerInput.IsAttackHeld) OnLeftClick(true);
    }

    void IssueMove(Vector3 point, bool held)
    {
        command = Command.Move;
        playerMovement.MoveTo(point);
        if (!held) playerTargeting.SpawnClickEffect(point);
    }

    void SetAttackTarget(InteractableUtils interactable)
    {
        currentTarget = interactable;
        command = Command.Attack;
        if (interactable.interactableType == InteractableType.Enemy)
            SelectTarget(interactable.stats);
    }

    // --- movement -------------------------------------------------------------

    void UpdateMovement()
    {
        if (movementLocked) { playerMovement.Stop(); return; }

        switch (command)
        {
            case Command.Move:
                if (playerMovement.HasArrived) command = Command.None;
                break;

            case Command.Attack:
                if (!TargetAlive()) { OnTargetDied(); break; }

                float dist = Vector3.Distance(currentTarget.transform.position, transform.position);
                if (dist > playerCombat.AttackRange)
                {
                    playerMovement.MoveTo(currentTarget.transform.position);   // guarded: only repaths when the target actually moves
                }
                else if (currentTarget.interactableType == InteractableType.Item)
                {
                    DoPickup();
                }
                else
                {
                    playerMovement.Stop();
                }
                break;
        }
    }

    void UpdateAutoAttack()
    {
        if (playerBusy || movementLocked) return;
        if (currentTarget == null || currentTarget.interactableType != InteractableType.Enemy) return;

        if (currentTarget.stats == null || currentTarget.stats.IsDead) { OnTargetDied(); return; }
        if (Vector3.Distance(currentTarget.transform.position, transform.position) > playerCombat.AttackRange) return;
        if (Time.time < nextAttackTime) return;

        float interval = playerCombat.AttackSpeed;

        // fast attacks speed the animation up; slow attacks play it once then idle until the next swing.
        float speedMultiplier = playerCombat.AttackAnimLength / interval;
        float clipDuration = playerCombat.AttackAnimLength / speedMultiplier;

        playerAnimator.PlayAttack(speedMultiplier);
        attackAnimUntil = Time.time + clipDuration;
        nextAttackTime = Time.time + interval;

        // Fire the hit (damage + VFX) at the configured point along the swing:
        // 1 = animation finished, 0.5 = halfway, 0 = start.
        Invoke(nameof(SendAttack), clipDuration * playerCombat.AnimationFireDamage);
    }

    void SendAttack()
    {
        if (!TargetAlive()) { ClearTarget(); return; }
        playerCombat.DealDamage(currentTarget);
    }

    // SoD attack-move: a target is "sticky" only while we're committed to it —
    // auto-attacking it in range, or attack-moving toward it. Once the swing
    // finishes and we've drifted out of range under a non-attack command, drop it.
    // Without this the player keeps facing (and selecting) an enemy they walked
    // away from, even 20ft out.
    void UpdateTargetRetention()
    {
        if (currentTarget == null || currentTarget.interactableType != InteractableType.Enemy) return;
        if (command == Command.Attack) return;     // still chasing it -> keep it
        if (Time.time < attackAnimUntil) return;   // mid-swing -> let the animation finish
        if (Vector3.Distance(currentTarget.transform.position, transform.position) <= playerCombat.AttackRange) return;

        DropTarget();
    }

    void DoPickup()
    {
        playerMovement.Stop();
        playerAnimator.PlayPickup();
        attackAnimUntil = Time.time + 0.5f;
        currentTarget.InteractWithItem();
        ClearTarget();
    }

    // --- facing / anims -------------------------------------------------------

    void UpdateFacing()
    {
        if (movementLocked) return;

        // While an attack is animating, turn to face the enemy (even when strafing/kiting).
        bool attacking = Time.time < attackAnimUntil;
        if ((attacking || playerMovement.IsStopped)
            && TargetAlive() && currentTarget.interactableType == InteractableType.Enemy)
        {
            playerMovement.FaceTowardsSmooth(currentTarget.transform.position);
        }
        else if (!playerMovement.IsStopped)
        {
            playerMovement.FaceMoveDirection();
        }
    }

    void SetAnimations()
    {
        // Base layer always reflects locomotion (legs). The upper-body layer,
        // driven by PlayAttack/PlayAbility, plays the swing over the top via mask.
        if (playerMovement.IsStopped) playerAnimator.PlayIdle();
        else playerAnimator.PlayWalk();
    }

    // --- target bookkeeping ---------------------------------------------------

    bool TargetAlive()
    {
        if (currentTarget == null) return false;
        if (currentTarget.interactableType == InteractableType.Item) return true;
        return currentTarget.stats != null && !currentTarget.stats.IsDead;
    }

    void ClearTarget()
    {
        currentTarget = null;
        if (command == Command.Attack) command = Command.None;
    }

    // SoD: when the enemy we're auto-attacking dies, don't just stop. If we're still
    // committed to attacking (the player hasn't issued a new move/attack command) and
    // another living enemy is still in auto-attack range, keep swinging — retargeting
    // the one closest to the PLAYER (no fresh cursor click here, so player-nearest wins).
    // Any RMB-move / LMB-elsewhere leaves command != Attack, so we just stop instead.
    void OnTargetDied()
    {
        if (command == Command.Attack
            && playerTargeting.FindEnemyNear(transform.position, playerCombat.AttackRange, out InteractableUtils next))
        {
            SetAttackTarget(next);
            return;
        }
        ClearTarget();
    }

    // Full release: stops us walking to / facing it AND clears the HUD selection.
    void DropTarget()
    {
        ClearTarget();        // currentTarget = null, command -> None
        SelectTarget(null);   // SelectedTarget = null (unsubscribes OnDied)
    }

    void SelectTarget(StatController target)
    {
        if (SelectedTarget != null) SelectedTarget.OnDied -= OnSelectedDied;

        SelectedTarget = target;

        if (SelectedTarget != null) SelectedTarget.OnDied += OnSelectedDied;
    }

    void OnSelectedDied(StatController s) => SelectedTarget = null;

    // --- ability hooks --------------------------------------------------------

    public void SetBusy(bool value)
    {
        playerBusy = value;
        if (!value) SetAnimations();
    }

    // Abilities flagged "lock movement" call this for the duration of the cast.
    public void SetMovementLocked(bool value)
    {
        movementLocked = value;
        if (value) playerMovement.Stop();
    }
}
