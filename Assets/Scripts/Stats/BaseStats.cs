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

        public float GetStat(Stat stat)
        {
            if (progression == null)
            {
                throw new InvalidOperationException($"Progression is not assigned on '{gameObject.name}'. Cannot get stat '{stat}'.");
            }

            return progression.GetStat(stat, characterClass, startingLevel);
        }

        public int GetLevel()
        {
            Experience experience = GetComponent<Experience>();
            if (experience == null) return startingLevel;

            float currentXP = experience.GetPoints();
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

