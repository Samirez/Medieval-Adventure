using UnityEngine;
using UnityEngine.Playables;
using RPG.Core;
using RPG.Control;

namespace RPG.Cinematics
{
    public class CinematicControlRemover : MonoBehaviour
    {
        GameObject player;
        private void Awake()
        {
            player = GameObject.FindWithTag("Player");
        }

        private void OnEnable()
        {
            PlayableDirector director = GetComponent<PlayableDirector>();
            if (director != null)
            {
                director.played += DisableControl;
                director.stopped += EnableControl;
            }
        }

        private void OnDisable()
        {
            PlayableDirector director = GetComponent<PlayableDirector>();
            if (director != null)
            {
                director.played -= DisableControl;
                director.stopped -= EnableControl;
            }
        }

        void DisableControl(PlayableDirector director)
        {
            player.GetComponent<ActionScheduler>().CancelCurrentAction();
            player.GetComponent<PlayerController>().enabled = false;
        }

        void EnableControl(PlayableDirector director)
        {
            player.GetComponent<PlayerController>().enabled = true;
        }
    }
}
