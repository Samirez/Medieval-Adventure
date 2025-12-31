using UnityEngine;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using RPG.Resources;
using System;
using GameDevTV.Utils;

namespace RPG.Control 
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 3f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance = 1f;
        [SerializeField] float waypointDwellTime = 2f;
        [SerializeField] float patrolSpeedFraction = 0.2f;
        Fighter fighter;
        GameObject player;
        Health health;
        Mover mover;

        LazyValue<Vector3> guardLocation;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        int currentWaypointIndex = 0;
        float timeSinceArrivedAtWaypoint = Mathf.Infinity;

        private void Awake()
        {
            fighter = GetComponent<Fighter>();
            if (fighter == null)
            {
                Debug.LogError($"Fighter component missing on '{gameObject.name}'. AIController requires a Fighter.");
            }

            player = GameObject.FindWithTag("Player");

            health = GetComponent<Health>();
            if (health == null)
            {
                Debug.LogError($"Health component missing on '{gameObject.name}'. AIController requires a Health.");
            }

            mover = GetComponent<Mover>();
            if (mover == null)
            {
                Debug.LogError($"Mover component missing on '{gameObject.name}'. AIController requires a Mover to patrol and move.");
            }
        }

        private void Start()
        {
            guardLocation = new LazyValue<Vector3>(GetGuardLocation);
            // Player may be instantiated after Awake in some setups; try a fallback lookup here.
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
                if (player == null)
                {
                    Debug.LogWarning("AIController: Player GameObject with tag 'Player' not found in Start(); AI behaviour that targets the player will be disabled.");
                }
            }
        }

        private Vector3 GetGuardLocation()
        {
            return transform.position;
        }

        private void Update()
        {
            if (health == null || health.IsDead()) return;

            // Ensure required references exist before attempting behavior that depends on them.
            bool canSeeAndAttack = false;
            if (player != null && fighter != null)
            {
                if (InAttackRangeOfPlayer() && fighter.CanAttack(player))
                {
                    canSeeAndAttack = true;
                }
            }

            if (canSeeAndAttack)
            {
                AttackBehavior();
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                SuspicionBehavior();
            }
            else
            {
                PatrolBehavior();
            }

            UpdateTimers();
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtWaypoint += Time.deltaTime;
        }

        private void AttackBehavior()
        {
            timeSinceLastSawPlayer = 0;
            if (fighter == null || player == null) return;
            fighter.Attack(player);
        }

        private void SuspicionBehavior()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void PatrolBehavior()
        {
            Vector3 nextPosition = guardLocation.value;
            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }
            if (timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                if (mover != null)
                {
                    mover.StartMoveAction(nextPosition, patrolSpeedFraction);
                }
                else
                {
                    Debug.LogWarning($"AIController on '{gameObject.name}' cannot patrol because Mover is missing.");
                }
            }
          
            
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetPosition(currentWaypointIndex);
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < waypointTolerance;
        }

        public bool InAttackRangeOfPlayer()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer < chaseDistance;
        }

        // called by Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    
    }

   
}

