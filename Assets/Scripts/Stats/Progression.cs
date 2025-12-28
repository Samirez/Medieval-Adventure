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
        // Deduplicating logger to avoid spamming the console with repeated messages.
        // Make this instance-scoped so each Progression asset deduplicates its own messages
        // and the set does not grow globally across domain reloads or play sessions.
        private HashSet<string> _loggedMessages;
        // Lock object used to make LogOnce thread-safe if called from multiple threads.
        // Note: initialize in OnEnable to avoid domain-reload issues with ScriptableObject instances.
        private object _logLock;

        private void OnEnable()
        {
            // initialize per-instance set when the ScriptableObject becomes enabled/loaded
            _loggedMessages = new HashSet<string>();
            // initialize lock here rather than inline to avoid issues across domain reloads
            if (_logLock == null) _logLock = new object();
        }

        private void OnDisable()
        {
            // clear to avoid holding onto memory across domain reloads or long editor sessions
            if (_logLock != null)
            {
                lock (_logLock)
                {
                    _loggedMessages?.Clear();
                    _loggedMessages = null;
                }
            }
        }

        private void LogOnce(string key, string message, UnityEngine.Object context = null, bool isError = false)
        {
            // key should uniquely identify the issue (e.g. characterclass/stat/level)
            lock (_logLock)
            {
                if (!_loggedMessages.Add(key)) return;
            }

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

        private void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();

            if (characterClasses == null) return;

            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, float[]>();

                if (progressionClass.stats == null) continue;

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

            if (!lookupTable.ContainsKey(characterClass))
            {
                var key = $"getLevels:missingClass:{characterClass}|{stat}";
                LogOnce(key, $"Progression has no entry for CharacterClass={characterClass}. Cannot get levels for Stat={stat}.", this);
                return 0;
            }

            var statLookup = lookupTable[characterClass];
            if (!statLookup.ContainsKey(stat))
            {
                var key = $"getLevels:missingStat:{characterClass}|{stat}";
                LogOnce(key, $"Progression for CharacterClass={characterClass} has no stat entry for Stat={stat}.", this);
                return 0;
            }

            var levels = statLookup[stat];
            if (levels == null) return 0;
            return levels.Length;
        }
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
