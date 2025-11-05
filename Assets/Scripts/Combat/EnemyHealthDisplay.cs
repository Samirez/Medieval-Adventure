using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using RPG.Resources;

namespace RPG.Combat 
{
    public class EnemyHealthDisplay : MonoBehaviour 
    {
        Fighter fighter;
        
        private void Awake()
        {
            fighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();
        }

        private void Update()
        {
            if (fighter.GetTarget() == null)
            {
                GetComponent<TextMeshProUGUI>().text = "N/A";
            }
            Health health = fighter.GetTarget();
            GetComponent<TextMeshProUGUI>().text = String.Format("{0:0}%",health.GetPercentage());
        }
    }
}