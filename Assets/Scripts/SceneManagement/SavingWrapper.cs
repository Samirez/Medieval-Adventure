using UnityEngine;
using System.Collections;
using RPG.Saving;

namespace RPG.SceneManagement 
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";
        [SerializeField] float fadeInTime = 0.2f;

        [System.Obsolete]
        IEnumerator Start()
        {
            Fader fader = FindFirstObjectByType<Fader>();
            fader.FadeOutImmediate();
            yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
            yield return fader.FadeIn(fadeInTime);
        }   

        [System.Obsolete]
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
        }

        [System.Obsolete]
        public void Load()
        {
            GetComponent<SavingSystem>().Load(defaultSaveFile);
        }

        [System.Obsolete]
        public void Save()
        {
            GetComponent<SavingSystem>().Save(defaultSaveFile);
        }
    }
}

