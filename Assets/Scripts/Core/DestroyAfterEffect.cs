using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RPG.Core 
{
    public class DestroyAfterEffect : MonoBehaviour
    {
    
        void Update()
        {
            if (!GetComponent<ParticleSystem>().IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
