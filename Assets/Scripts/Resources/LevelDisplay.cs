using UnityEngine;
using TMPro;
using System;
using RPG.Stats;

public class LevelDisplay : MonoBehaviour
{
    int level;

    private void Awake()
    {
        level = GameObject.FindWithTag("Player").GetComponent<BaseStats>().GetLevel();
    }

    public void Update()
    {
        GetComponent<TextMeshProUGUI>().text = String.Format("{0:0}", level);
    }
}