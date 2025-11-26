using UnityEngine;
using RPG.Saving;

namespace RPG.Resources
{
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] float experiencePoints = 0;

        public event Action onExperienceGained;

        public float ExperiencePoints => experiencePoints;

        public object CaptureState()
        {
            return experiencePoints;
        }

        public void RestoreState(object state)
        {
            experiencePoints = (float)state;
        }

        public float GetPoints()
        {
            return experiencePoints;
        }
        
        public void GainExperience(float experience)
        {
            if (experience < 0)
            {
                Debug.LogWarning("Cannot gain negative experience");
                return;
            }
            experiencePoints += experience;
            onExperienceGained?.Invoke();
        }


    }
}

