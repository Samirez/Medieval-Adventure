using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]

    public class Progression : ScriptableObject 
    {
        [SerializeField] ProgressionCharacterClass[] characterClasses = null;
        Dictionary<CharacterClass, Dictionary<Stat, float[]>> lookupTable = null;
        // Deduplicating logger to avoid spamming the console with repeated messages
        private static HashSet<string> s_loggedMessages = new HashSet<string>();

        private void LogOnce(string key, string message, UnityEngine.Object context = null, bool isError = false)
        {
            // key should uniquely identify the issue (e.g. characterclass/stat/level)
            if (!s_loggedMessages.Add(key)) return;

            if (isError)
            {
                Debug.LogError(message, context);
#if UNITY_EDITOR
                // Fail fast in editor for integrity issues so developers notice them immediately
                throw new InvalidOperationException(message);
#endif
            }
            else
            {
                Debug.LogWarning(message, context);
            }
        }

        public float GetStat(Stat stat, CharacterClass characterclass, int level)
        {
            BuildLookup();

            // Defensive: ensure we have data for the requested character class and stat
            if (lookupTable == null)
            {
                var key = $"lookupTable:null|{characterclass}|{stat}|{level}";
                var msg = $"Progression lookup table is not built or is empty. Missing data for CharacterClass={characterclass}, Stat={stat}, Level={level}.";
                LogOnce(key, msg, this, isError: true);
                return 0f;
            }

            if (!lookupTable.ContainsKey(characterclass))
            {
                var key = $"missingClass:{characterclass}|{stat}|{level}";
                var msg = $"Progression has no entry for CharacterClass={characterclass}. Requested Stat={stat}, Level={level}.";
                LogOnce(key, msg, this, isError: true);
                return 0f;
            }

            var statLookup = lookupTable[characterclass];

            if (!statLookup.ContainsKey(stat))
            {
                var key = $"missingStat:{characterclass}|{stat}|{level}";
                var msg = $"Progression for CharacterClass={characterclass} has no stat entry for Stat={stat}. Requested Level={level}.";
                LogOnce(key, msg, this, isError: true);
                return 0f;
            }

            var levels = statLookup[stat];
            if (levels == null || levels.Length == 0)
            {
                var key = $"emptyLevels:{characterclass}|{stat}|{level}";
                var msg = $"Progression levels array is null or empty for CharacterClass={characterclass}, Stat={stat}. Requested Level={level}.";
                LogOnce(key, msg, this, isError: true);
                return 0f;
            }

            // level is expected to be 1-based. Validate the requested level.
            if (level <= 0)
            {
                var key = $"invalidLevel:{characterclass}|{stat}|{level}";
                var msg = $"Requested level {level} is invalid (must be >= 1) for CharacterClass={characterclass}, Stat={stat}.";
                LogOnce(key, msg, this, isError: false);
                return 0f;
            }

            if (levels.Length < level)
            {
                var key = $"levelOutOfRange:{characterclass}|{stat}|{level}";
                var msg = $"Requested level {level} is out of range (max {levels.Length}) for CharacterClass={characterclass}, Stat={stat}.";
                LogOnce(key, msg, this, isError: false);
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

        public int GetLevels(Stat stat, CharacterClass characterClass)
        {
            BuildLookup();

            if (lookupTable == null)
            {
                Debug.LogWarning($"Progression lookup table is not built or is empty. Cannot get levels for CharacterClass={characterClass}, Stat={stat}.", this);
                return 0;
            }

            if (!lookupTable.ContainsKey(characterClass))
            {
                Debug.LogWarning($"Progression has no entry for CharacterClass={characterClass}. Cannot get levels for Stat={stat}.", this);
                return 0;
            }

            var statLookup = lookupTable[characterClass];
            if (!statLookup.ContainsKey(stat))
            {
                Debug.LogWarning($"Progression for CharacterClass={characterClass} has no stat entry for Stat={stat}.", this);
                return 0;
            }

            var levels = statLookup[stat];
            if (levels == null) return 0;
            return levels.Length;
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
