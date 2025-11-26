using UnityEngine;
using TMPro;
using System;
using RPG.Stats;

public class LevelDisplay : MonoBehaviour
{
    int level;
    private TextMeshProUGUI levelText;

    private void Awake()
    {
        // Cache the TMP component once
        levelText = GetComponent<TextMeshProUGUI>();
        if (levelText == null)
        {
            Debug.LogError("LevelDisplay requires a TextMeshProUGUI component on the same GameObject.");
            enabled = false;
            return;
        }

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("No GameObject found with tag 'Player' for LevelDisplay. Disabling.");
            enabled = false;
            return;
        }

        if (!player.TryGetComponent<BaseStats>(out var baseStats))
        {
            Debug.LogError("Player GameObject does not have BaseStats component. LevelDisplay disabled.");
            enabled = false;
            return;
        }

        level = baseStats.GetLevel();
        // Set initial level text once. Use zero-padded two-digit format.
        levelText.text = String.Format("{0:00}", level);
    }
}