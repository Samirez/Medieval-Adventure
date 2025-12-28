using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using RPG.Resources;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 99)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null;

        // Ensure currentLevel is never zero to avoid level-0 edge cases when other scripts query early.
        int currentLevel = 1;
        Experience experience;
        float lastXP;

        private void Awake()
        {
            // Assign from serialized startingLevel (set by inspector) so Awake-time queries see the correct base level.
            currentLevel = startingLevel;
        }

        private void OnEnable()
        {
            // Subscribe to experience events here so enable/disable cycles correctly
            // restore event handlers and experience state.
            experience = GetComponent<Experience>();
            if (experience != null)
            {
                experience.onExperienceGained -= UpdateLevel; // ensure no duplicate subscriptions
                experience.onExperienceGained += UpdateLevel;
                lastXP = experience.ExperiencePoints;
            }
        }

        private void OnDisable()
        {
            if (experience != null)
            {
                experience.onExperienceGained -= UpdateLevel;
            }
        }

        private void Start()
        {
            // One-time setup that should only run once per object lifetime.
            if (experience == null)
            {
                experience = GetComponent<Experience>();
            }

            currentLevel = CalculateLevel();
        }

        private void UpdateLevel()
        {
            if (experience == null) return;
            float currentXP = experience.ExperiencePoints;
            if (Mathf.Approximately(currentXP, lastXP)) return;

            lastXP = currentXP;
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;
                Debug.Log($"Leveled up to {currentLevel}!");
            }
        }
        public float GetStat(Stat stat)
        {
            if (progression == null)
            {
                throw new InvalidOperationException($"Progression is not assigned on '{gameObject.name}'. Cannot get stat '{stat}'.");
            }

            return progression.GetStat(stat, characterClass, GetLevel());
        }

        public int GetLevel()
        {
            if (currentLevel < 1)
            {
                currentLevel = CalculateLevel();
            }
            
            return currentLevel;
        }

        public int CalculateLevel()
        {
            // Prefer the cached `experience` reference set in OnEnable/Start.
            Experience exp = experience ?? GetComponent<Experience>();
            if (exp == null) return startingLevel;

            float currentXP = exp.ExperiencePoints;
            if (progression == null)
            {
                throw new InvalidOperationException($"Progression is not assigned on '{gameObject.name}'. Cannot determine level for CharacterClass={characterClass}.");
            }

            int MaxLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);

            for (int levels = 1; levels <= MaxLevel; levels++)
            {
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, levels);
                if (currentXP < XPToLevelUp)
                {
                    return levels;
                }
            }

            // If we've reached or exceeded all thresholds, return the maximum defined level.
            return MaxLevel;
        }

    }
}

