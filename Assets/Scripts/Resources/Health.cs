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
            health = GetComponent<BaseStats>().GetStat(Stat.Health);   
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
            return 100 * (health / GetComponent<BaseStats>().GetStat(Stat.Health));
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

            float xpReward = baseStats.GetStat(Stat.ExperienceReward);
            experience.GainExperience(xpReward);
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

