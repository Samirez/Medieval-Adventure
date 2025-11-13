using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Stats
{
    public class ExperienceDisplay : MonoBehaviour
    {
        Experience experience;

        private void Awake()
        {
           experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
        }

        private void Update()
        {
            if (experience == null)
            {
                Debug.LogError("Player GameObject does not have an Experience component.");
                return;
            }

            GetComponent<Text>().text = String.Format("{0:0}", experience.ExperiencePoints);
        }
    }
}