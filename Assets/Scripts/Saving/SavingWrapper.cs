using UnityEngine;
using RPG.Saving;

namespace RPG.Saving
{   
    public class SavingWrapper : MonoBehaviour
    {
        private SavingSystem savingSystem;
        [SerializeField] string saveFile = "save";

        // Cache the SavingSystem reference early to avoid repeated GetComponent calls
        private void Awake()
        {
            savingSystem = GetComponent<SavingSystem>();
            if (savingSystem == null)
            {
                Debug.LogError("SavingWrapper: SavingSystem component not found on the same GameObject. Disabling SavingWrapper.");
                enabled = false;
                return;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Delete();
            }
        }

        public void Save()
        {
            if (!EnsureSavingSystem("save")) return;
            #pragma warning disable CS0612
            savingSystem.Save(saveFile);
            #pragma warning restore CS0612
        }

        public void Load()
        {
            if (!EnsureSavingSystem("load")) return;
            #pragma warning disable CS0612
            savingSystem.Load(saveFile);
            #pragma warning restore CS0612
        }

        public void Delete()
        {
            if (!EnsureSavingSystem("delete")) return;
            #pragma warning disable CS0612
            savingSystem.Delete(saveFile);
            #pragma warning restore CS0612
        }

        private bool EnsureSavingSystem(string operationName)
        {
            if (savingSystem == null)
            {
                Debug.LogWarning($"Cannot {operationName}: SavingSystem reference is missing.");
                return false;
            }
            return true;
        }
    }
}
