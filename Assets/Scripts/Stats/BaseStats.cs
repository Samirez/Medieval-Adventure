using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
                Debug.LogError($"Progression is not assigned on {gameObject.name}. Cannot get stat {stat}.");
                return 0f;
            }

            return progression.GetStat(stat, characterClass, startingLevel);
        }

    }
}

