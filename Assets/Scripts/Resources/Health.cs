using System;
using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;

namespace RPG.Resources
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] float health = 100f;
        [SerializeField] float DelayBeforeDestroy = 5f;
        bool isDead = false;

        private void Start()
        {
            BaseStats baseStats = GetComponent<BaseStats>();
            if (baseStats == null)
            {
                Debug.LogError($"BaseStats component missing on {gameObject.name} during Start. Health cannot be initialized.");
                return;
            }

            try
            {
                health = baseStats.GetStat(Stat.Health);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"Failed to initialize health for {gameObject.name}: {ex.Message}");
            }
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void TakeDamage(GameObject instigator, float damage)
        {
            health = Mathf.Max(health - damage, 0);
            if (health <= 0)
            {
                Die();
                GrantExperience(instigator);
            }
        }


        public float GetPercentage()
        {
            BaseStats baseStats = GetComponent<BaseStats>();
            if (baseStats == null)
            {
                Debug.LogError($"BaseStats component missing on {gameObject.name} when calculating health percentage.");
                return 0f;
            }

            try
            {
                float maxHealth = baseStats.GetStat(Stat.Health);
                if (Mathf.Approximately(maxHealth, 0f)) return 0f;
                return 100 * (health / maxHealth);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"Failed to calculate health percentage for {gameObject.name}: {ex.Message}");
                return 0f;
            }
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
        
        private void GrantExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();
            if (experience == null) return;

            // Get this object's BaseStats and fetch the numeric experience reward
            BaseStats baseStats = GetComponent<BaseStats>();
            if (baseStats == null)
            {
                Debug.LogWarning($"BaseStats component missing on {gameObject.name}; cannot grant experience.");
                return;
            }

            try
            {
                float xpReward = baseStats.GetStat(Stat.ExperienceReward);
                experience.GainExperience(xpReward);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"Failed to grant experience for {gameObject.name}: {ex.Message}");
            }
        }

        public object CaptureState()
        {
            return health;
        }

        public void RestoreState(object state)
        {
            health = (float)state;
            if (health <= 0)
            {
                Die();
            }
        }
    }
}

