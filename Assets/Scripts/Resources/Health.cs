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
            health = GetComponent<BaseStats>().GetHealth();   
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
            return 100 * (health / GetComponent<BaseStats>().GetHealth());
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
            Experience experience = instigator.GetComponent<experience>();
            if (experience == null) return;

            experience.GainExperience(GetComponent<BaseStats>().GetExperienceReward());
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

