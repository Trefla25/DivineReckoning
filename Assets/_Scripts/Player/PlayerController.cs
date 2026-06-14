using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
   const string IDLE = "Idle";
   const string WALK = "Walk";
   const string ATTACK = "Attack";
   const string PICKUP = "Pickup";

   CustomInputs input;

   NavMeshAgent agent;
   Animator animator;

   [Header("Movement")]
   [SerializeField] ParticleSystem clickEffect;
   [SerializeField] LayerMask clickableLayers;
   [SerializeField] float holdRepathInterval = 0.1f;

   [Header("BasicAttack")]
   [SerializeField] float attackSpeed = 1.5f;
   [SerializeField] float attackDelay = 0.3f;
   [SerializeField] float attackRange = 1.5f;
   [SerializeField] int attackDamage = 1;
   [SerializeField] ParticleSystem attackEffect;
   bool playerBusy = false;
   Interactable currentTarget;
   

   float lookRotationSpeed = 8f;
   float nextRepathTime;
   
   void Awake() 
   {
      agent = GetComponent<NavMeshAgent>();
      animator = GetComponent<Animator>();

      input = new CustomInputs();
      AssignInputs();
   }

   void AssignInputs()
   {
      input.Main.MoveClick.performed += ctx => ClickToMove(false);
   }

   void ClickToMove(bool pressed)
   {
      nextRepathTime = Time.time + holdRepathInterval;

      Vector2 mousePosition = Mouse.current.position.ReadValue();
      if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition),
      out RaycastHit hit, 100, clickableLayers))
      {
         if (hit.transform.CompareTag("Interactable"))
         {
            currentTarget = hit.transform.GetComponent<Interactable>();
            if(clickEffect != null && !pressed)
            {
               Instantiate(clickEffect, hit.point + new Vector3(0, 0.1f, 0),
                           clickEffect.transform.rotation); 
            }
         }
         else
         {
            currentTarget = null;

            agent.destination = hit.point;
            if(clickEffect != null && !pressed)
            { 
               Instantiate(clickEffect, hit.point + new Vector3(0, 0.1f, 0),
                           clickEffect.transform.rotation); 
            }            
         }
      }
   }

   void HoldToMove()
   {
      if (input.Main.MoveClick.IsPressed() && Time.time >= nextRepathTime)
      { 
         ClickToMove(true);
      }
   }

   void OnEnable() 
   { 
      input.Enable(); 
   }

   void OnDisable() 
   { 
      input.Disable();
   }

   void Update()
   {
      FollowTarget();
      HoldToMove();
      FaceTarget();
      SetAnimations();
   }

   void FollowTarget()
   {
      if(currentTarget == null) return;

      if(Vector3.Distance(currentTarget.transform.position, transform.position) <= attackRange)
      {
         ReachDistance();
      }
      else
      {
         agent.SetDestination(currentTarget.transform.position);
      }
   }

   void FaceTarget()
   {
      Vector3 direction = agent.desiredVelocity;
      Vector3 flatDirection = new(direction.x, 0, direction.z);
      if (flatDirection.sqrMagnitude < 0.001f) return;

      Quaternion lookRotation = Quaternion.LookRotation(flatDirection);
      transform.rotation = Quaternion.Slerp(transform.rotation,
                                             lookRotation,
                                             Time.deltaTime * lookRotationSpeed);
   }

   void ReachDistance()
   {
      agent.SetDestination(transform.position);

      if(playerBusy) return;

      playerBusy = true;

      switch (currentTarget.interactableType)
      {
         case InteractableType.Enemy:
            animator.Play(ATTACK);

            Invoke(nameof(SendAttack), attackDelay);
            Invoke(nameof(ResetBusyState), attackSpeed);
            break;
         case InteractableType.Item:
            animator.Play(PICKUP);

            currentTarget.InteractWithItem();
            currentTarget = null;

            Invoke(nameof(ResetBusyState), 0.5f);
            break;
      }
   }

   void SendAttack()
   {
      if(currentTarget == null) return;

      if(currentTarget.damageActor.currentHealth <= 0)
      {
         currentTarget = null;
         return;
      }

      Instantiate(attackEffect, currentTarget.transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
      currentTarget.GetComponent<DealDamageActor>().TakeDamage(attackDamage);
   }

   void ResetBusyState()
   {
      playerBusy = false;
      SetAnimations();
   }

   void SetAnimations()
   {
      if(playerBusy) return;

      if(agent.velocity == Vector3.zero)
      { 
         animator.Play(IDLE); 
      }
      else
      {
          animator.Play(WALK);
      }
   }
}