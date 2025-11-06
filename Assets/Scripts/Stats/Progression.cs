using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]

    public class Progression : ScriptableObject 
    {
        [SerializeField] ProgressionCharacterClass[] characterClasses = null;

        public float GetHealth(CharacterClass characterclass, int level)
        {
            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                if (progressionClass.CharacterClass == characterclass)
                {
                    if (progressionClass.stats == null || progressionClass.stats.Length == 0)
                    {
                        Debug.LogWarning($"No stats defined for character class {characterclass} in Progression.");
                        return 0f;
                    }

                    // Find the ProgressionStat entry for Health
                    ProgressionStat healthStat = null;
                    foreach (ProgressionStat ps in progressionClass.stats)
                    {
                        if (ps == null) continue;
                        if (ps.stat == Stat.Health)
                        {
                            healthStat = ps;
                            break;
                        }
                    }

                    if (healthStat == null)
                    {
                        Debug.LogWarning($"Health stat not found for character class {characterclass} in Progression.");
                        return 0f;
                    }

                    if (healthStat.levels == null || healthStat.levels.Length < level || level <= 0)
                    {
                        Debug.LogWarning($"Requested level {level} is out of bounds for Health levels on character class {characterclass} in Progression.");
                        return 0f;
                    }

                    return healthStat.levels[level - 1];
                }
            }
            return 0;
        }

        [System.Serializable]
        class ProgressionCharacterClass
        {
            public CharacterClass CharacterClass;
            public ProgressionStat[] stats; 
            //public float[] Health;
        }

        [System.Serializable]
        class ProgressionStat
        {
            public Stat stat;
            public float[] levels;
        }
    }
}
