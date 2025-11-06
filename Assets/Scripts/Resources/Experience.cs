using UnityEngine;

namespace RPG.Resources
{
    public class Experience : MonoBehaviour
    {
        [SerializeField] float experiencePoints = 0;

        public float ExperiencePoints => experiencePoints;

        public void GainExperience(float experience)
        {
            if (experience < 0)
            {
                Debug.LogWarning("Cannot gain negative experience");
                return;
            }
            experiencePoints += experience;
        }

    }
}

