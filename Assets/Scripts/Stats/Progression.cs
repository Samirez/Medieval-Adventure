using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]

    public class Progression : ScriptableObject 
    {
        [SerializeField] ProgressionCharacterClass[] characterClasses = null;
        Dictionary<CharacterClass, Dictionary<Stat, float[]>> lookupTable = null;

        public float GetStat(Stat stat, CharacterClass characterclass, int level)
        {
            BuildLookup();

            // Defensive: ensure we have data for the requested character class and stat
            if (lookupTable == null)
            {
                Debug.LogWarning($"Progression lookup table is not built or is empty. Missing data for CharacterClass={characterclass}, Stat={stat}, Level={level}.", this);
                return 0f;
            }

            if (!lookupTable.ContainsKey(characterclass))
            {
                Debug.LogWarning($"Progression has no entry for CharacterClass={characterclass}. Requested Stat={stat}, Level={level}.", this);
                return 0f;
            }

            var statLookup = lookupTable[characterclass];

            if (!statLookup.ContainsKey(stat))
            {
                Debug.LogWarning($"Progression for CharacterClass={characterclass} has no stat entry for Stat={stat}. Requested Level={level}.", this);
                return 0f;
            }

            var levels = statLookup[stat];
            if (levels == null || levels.Length == 0)
            {
                Debug.LogWarning($"Progression levels array is null or empty for CharacterClass={characterclass}, Stat={stat}. Requested Level={level}.", this);
                return 0f;
            }

            // level is expected to be 1-based. Validate the requested level.
            if (level <= 0)
            {
                Debug.LogWarning($"Requested level {level} is invalid (must be >= 1) for CharacterClass={characterclass}, Stat={stat}.", this);
                return 0f;
            }

            if (levels.Length < level)
            {
                Debug.LogWarning($"Requested level {level} is out of range (max {levels.Length}) for CharacterClass={characterclass}, Stat={stat}.", this);
                return 0f;
            }

            return levels[level - 1];
        }

        public void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();

            if (characterClasses == null) return;

            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, float[]>();

                foreach (ProgressionStat progressionStat in progressionClass.stats)
                {
                    statLookupTable[progressionStat.stat] = progressionStat.levels;
                }

                lookupTable[progressionClass.CharacterClass] = statLookupTable;
            }
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
