using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
   const string IDLE = "Idle";
   const string WALK = "Walk";

   CustomInputs input;

   NavMeshAgent agent;
   Animator animator;

   [Header("Movement")]
   [SerializeField] ParticleSystem clickEffect;
   [SerializeField] LayerMask clickableLayers;
   [SerializeField] float holdRepathInterval = 0.1f;

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
      input.Main.MoveClick.performed += ctx => ClickToMove(true);
   }

   void ClickToMove(bool spawnEffect)
   {
      nextRepathTime = Time.time + holdRepathInterval;

      Vector2 mousePosition = Mouse.current.position.ReadValue();
      if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition),
                           out RaycastHit hit,
                           100,
                           clickableLayers))
      {
         agent.destination = hit.point;
         if(spawnEffect && clickEffect != null)
         { Instantiate(clickEffect, hit.point + new Vector3(0, 0.1f, 0),
                        clickEffect.transform.rotation); }
      }
   }

   void HoldToMove()
   {
      if (input.Main.MoveClick.IsPressed() && Time.time >= nextRepathTime)
      { 
         ClickToMove(false); 
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
      HoldToMove();
      FaceTarget();
      SetAnimations();
   }

   void FaceTarget()
   {
      Vector3 direction = (agent.destination - transform.position).normalized;
      Vector3 flatDirection = new(direction.x, 0, direction.z);
      if (flatDirection == Vector3.zero) return;

      Quaternion lookRotation = Quaternion.LookRotation(flatDirection);
      transform.rotation = Quaternion.Slerp(transform.rotation,
                                             lookRotation,
                                             Time.deltaTime * lookRotationSpeed);
   }

   void SetAnimations()
   {
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