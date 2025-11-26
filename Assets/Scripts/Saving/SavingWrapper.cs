using UnityEngine;

public class SavingWrapper : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
        GetComponent<SavingSystem>().Save();
    }

    public void Load()
    {
        GetComponent<SavingSystem>().Load();
    }

    public void Delete()
    {
        GetComponent<SavingSystem>().Delete();
    }
}
