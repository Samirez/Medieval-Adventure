using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;
using RPG.Resources;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6f;
        NavMeshAgent navMeshAgent;
        Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            if (navMeshAgent == null)
            {
                Debug.LogError("NavMeshAgent component is missing on " + gameObject.name);
            }
        }

        void Update()
        {
            navMeshAgent.enabled = !health.IsDead();
            UpdateAnimator();   
        }

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            if (navMeshAgent == null) return; // Prevent null reference
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination, speedFraction);
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            if (navMeshAgent == null) return; // Prevent null reference
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        public void Cancel()
        {
            if (navMeshAgent == null) return; // Prevent null reference
            navMeshAgent.isStopped = true;
        }

        private void UpdateAnimator()
        {
            Animator animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component is missing on " + gameObject.name);
                return;
            }

            Vector3 velocity = navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            animator.SetFloat("ForwardSpeed", speed);
        }

        public object CaptureState()
        {
            return new SerializableVector3(transform.position);
        }

        public void RestoreState(object state)
        {
          SerializableVector3 position = (SerializableVector3)state;
          navMeshAgent.enabled = false;
          transform.position = position.ToVector();
          navMeshAgent.enabled = true;
          GetComponent<ActionScheduler>().CancelCurrentAction();
        }
    }
}