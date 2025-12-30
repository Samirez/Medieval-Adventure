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
        [SerializeField] GameObject levelUpEffect = null;

        public event Action onLevelUp;

        // Ensure currentLevel is never zero to avoid level-0 edge cases when other scripts query early.
        int currentLevel = 1;
        Experience experience;
        float lastXP;
        IModifierProvider[] modifierProviders;

        private void Awake()
        {
            // Assign from serialized startingLevel (set by inspector) so Awake-time queries see the correct base level.
            currentLevel = startingLevel;
            // Cache the Experience component once during Awake to avoid repeated GetComponent calls.
            experience = GetComponent<Experience>();
            // Cache modifier providers to avoid allocations from GetComponents at runtime.
            modifierProviders = GetComponents<IModifierProvider>();
        }

        private void OnEnable()
        {
            // Subscribe to experience events here so enable/disable cycles correctly
            // restore event handlers and experience state. Use cached `experience`.
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
                LevelUpEffect();
                onLevelUp?.Invoke();
            }
        }

        private void LevelUpEffect()
        {
            Instantiate(levelUpEffect, transform);    
        }


        public float GetStat(Stat stat)
        {
            return (GetBaseStat(stat) + GetAdditiveModifier(stat))*(1 + GetPercentageModifier(stat)/100);
        }

        public float GetBaseStat(Stat stat)
        {
            if (progression == null)
            {
                throw new InvalidOperationException($"Progression is not assigned on '{gameObject.name}'. Cannot get base stat '{stat}'.");
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

        private float GetAdditiveModifier(Stat stat)
        {
            float total = 0;
            if (modifierProviders == null || modifierProviders.Length == 0)
            {
                modifierProviders = GetComponents<IModifierProvider>();
            }

            foreach (IModifierProvider provider in modifierProviders)
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        private float GetPercentageModifier(Stat stat)
        {
            float total = 0;
            if (modifierProviders == null || modifierProviders.Length == 0)
            {
                modifierProviders = GetComponents<IModifierProvider>();
            }

            foreach (IModifierProvider provider in modifierProviders)
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        // Call this if modifier providers are added/removed at runtime and the cache needs refreshing.
        public void RefreshModifierProviders()
        {
            modifierProviders = GetComponents<IModifierProvider>();
        }

        private int CalculateLevel()
        {
            // Prefer the cached `experience` reference; if it's missing, cache the component here
            // to avoid repeated GetComponent lookups on subsequent calls.
            if (experience == null)
            {
                experience = GetComponent<Experience>();
            }
            if (experience == null) return startingLevel;

            float currentXP = experience.ExperiencePoints;
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

