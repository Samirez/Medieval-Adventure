using System;
using TMPro;
using UnityEngine;
using RPG.Resources;

namespace RPG.Stats
{
    public class ExperienceDisplay : MonoBehaviour
    {
        private Experience experience;
        private TextMeshProUGUI experienceText;

        private void Awake()
        {
            // Cache the UI Text component once
            experienceText = GetComponent<TextMeshProUGUI>();
            if (experienceText == null)
            {
                Debug.LogError("ExperienceDisplay requires a TextMeshProUGUI component on the same GameObject.");
                enabled = false;
                return;
            }

            // Safely find the player and its Experience component
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogError("No GameObject found with tag 'Player'. ExperienceDisplay will be disabled.");
                enabled = false;
                return;
            }

            if (!playerGO.TryGetComponent<Experience>(out experience))
            {
                Debug.LogError("Player GameObject does not have an Experience component. ExperienceDisplay will be disabled.");
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            experienceText.text = experience.GetPoints().ToString("0");
        }
    }
}