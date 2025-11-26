using System;
using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;

namespace RPG.Resources
{
    public class Health : MonoBehaviour, ISaveable
    {
        float health = -1f;
        bool isDead = false;

        private void Start()
        {
            if (health < 0)
            {
                BaseStats baseStats = GetComponent<BaseStats>();
                if (baseStats == null)
                {
                    Debug.LogWarning($"BaseStats component missing on {gameObject.name} during Start. Using fallback health value of 1.");
                    health = 1f;
                    isDead = false;
                    return;
                }

                try
                {
                    health = baseStats.GetStat(Stat.Health);
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogWarning($"Failed to initialize health for {gameObject.name}: {ex.Message}. Using fallback health value of 1.");
                    health = 1f;
                    isDead = false;
                }
            }
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void TakeDamage(GameObject instigator, float damage)
        {
            // Ensure health is initialized to a sensible positive value before applying damage
            if (health < 0f)
            {
                Debug.LogWarning($"Health for {gameObject.name} was uninitialized (value {health}). Falling back to 1 before applying damage.");
                health = 1f;
            }

            health = Mathf.Max(health - damage, 0f);

            if (health <= 0f)
            {
                Die();
                GrantExperience(instigator);
            }
        }


        public float GetPercentage()
        {
            // Determine maximum health safely. If BaseStats is missing or misconfigured, fall back to 1.
            float maxHealth = 1f;
            BaseStats baseStats = GetComponent<BaseStats>();
            if (baseStats == null)
            {
                Debug.LogWarning($"BaseStats component missing on {gameObject.name} when calculating health percentage. Using fallback maxHealth=1.");
            }
            else
            {
                try
                {
                    maxHealth = baseStats.GetStat(Stat.Health);
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogWarning($"Failed to obtain max health for {gameObject.name}: {ex.Message}. Using fallback maxHealth=1.");
                    maxHealth = 1f;
                }
            }

            // Guard against invalid maxHealth values
            if (Mathf.Approximately(maxHealth, 0f) || maxHealth < 0f)
            {
                Debug.LogWarning($"Computed maxHealth for {gameObject.name} is invalid ({maxHealth}). Using fallback maxHealth=1.");
                maxHealth = 1f;
            }

            // Ensure current health is within [0, maxHealth]
            if (health < 0f)
            {
                Debug.LogWarning($"Health for {gameObject.name} is negative ({health}) when computing percentage. Clamping to 0.");
                health = 0f;
            }
            health = Mathf.Clamp(health, 0f, maxHealth);

            return 100f * (health / maxHealth);
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

