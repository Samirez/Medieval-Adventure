using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

    }
}

