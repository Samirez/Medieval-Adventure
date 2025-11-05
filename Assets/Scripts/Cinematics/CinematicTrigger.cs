using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour
    {
        bool alreadyTriggered = false;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                PlayableDirector playableDirector = GetComponent<PlayableDirector>();
                if (playableDirector == null)
                {
                    Debug.LogError("PlayableDirector component not found!");
                    return;
                }

                if (!alreadyTriggered)
                {
                    alreadyTriggered = true;
                    playableDirector.Play();
                }
            }
        }
    }
}

