using System;
using GameDevTV.Utils;
using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;

namespace RPG.Resources
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] float regenerationPercentage = 70f;
        LazyValue<float> health;
        BaseStats baseStats;
        bool isDead = false;


        private void Awake()
        {
            baseStats = GetComponent<BaseStats>();
            health = new LazyValue<float>(GetInitialHealth);
        }

        private float GetInitialHealth()
        {
            BaseStats baseStats = GetComponent<BaseStats>();
            if (baseStats == null)
            {
                Debug.LogWarning($"BaseStats component missing on {gameObject.name} during health initialization. Using fallback health value of 1.");
                return 1f;
            }

            try
            {
                return baseStats.GetStat(Stat.Health);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"Failed to initialize health for {gameObject.name}: {ex.Message}. Using fallback health value of 1.");
                return 1f;
            }
        }
        
        private void Start()
        {
            if (baseStats == null) baseStats = GetComponent<BaseStats>();
            if (baseStats != null)
            {
                baseStats.onLevelUp += UpdateHealthOnLevelUp;
            }
            else
            {
                Debug.LogWarning($"BaseStats component missing on {gameObject.name}; level-up health regeneration disabled.");
            }

            // Ensure the lazy health value is initialized
            try
            {
                health.ForceInit();
            }
            catch
            {
                // Fallback if LazyValue doesn't support ForceInit for some reason
                health.value = GetInitialHealth();
            }

            isDead = health.value <= 0f;
        }
        public float GetHealthPoints()
        {
            return health.value;
        }

        public float GetMaxHealthPoints()
        {
            BaseStats baseStats = GetComponent<BaseStats>();
            if (baseStats == null)
            {
                Debug.LogWarning($"BaseStats component missing on {gameObject.name} when getting max health. Returning fallback value of 1.");
                return 1f;
            }

            try
            {
                return baseStats.GetStat(Stat.Health);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"Failed to get max health for {gameObject.name}: {ex.Message}. Returning fallback value of 1.");
                return 1f;
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

            // Compute percentage without mutating object state (command-query separation)
            float current = health.value;
            if (current < 0f)
            {
                Debug.LogWarning($"Health for {gameObject.name} is negative ({current}) when computing percentage. Using 0 for display calculations.");
            }

            float clampedHealth = Mathf.Clamp(current, 0f, maxHealth);

            return 100f * (clampedHealth / maxHealth);
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

        private void UpdateHealthOnLevelUp()
        {
            if (baseStats == null)
            {
                baseStats = GetComponent<BaseStats>();
                if (baseStats == null)
                {
                    Debug.LogWarning($"BaseStats component missing on {gameObject.name}; cannot update health on level up.");
                    return;
                }
            }

            float newHealth = baseStats.GetStat(Stat.Health) * regenerationPercentage / 100f;
            health.value = Mathf.Max(health.value, newHealth);
        }

        public object CaptureState()
        {
            return health.value;
        }

        public void RestoreState(object state)
        {
            health.value = (float)state;
            if (health.value <= 0)
            {
                Die();
            }
        }
    }
}

