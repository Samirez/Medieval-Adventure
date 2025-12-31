using UnityEngine;
using UnityEngine.Playables;
using RPG.Core;
using RPG.Control;

namespace RPG.Cinematics
{
    public class CinematicControlRemover : MonoBehaviour
    {
        GameObject player;
        PlayableDirector director;
        ActionScheduler actionScheduler;
        PlayerController playerController;
        private void Awake()
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player GameObject not found. Ensure a GameObject is tagged as 'Player'.");
            }
            director = GetComponent<PlayableDirector>();
            if (director == null)
            {
                Debug.LogWarning($"PlayableDirector component not found on '{gameObject.name}'. CinematicControlRemover will not function.");
            }

            if (player != null)
            {
                actionScheduler = player.GetComponent<ActionScheduler>();
                if (actionScheduler == null)
                {
                    Debug.LogWarning($"Player GameObject '{player.name}' is missing an ActionScheduler component.");
                }

                playerController = player.GetComponent<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogWarning($"Player GameObject '{player.name}' is missing a PlayerController component.");
                }
            }
        }

        private void OnEnable()
        {
            if (director != null)
            {
                director.played += DisableControl;
                director.stopped += EnableControl;
            }
        }

        private void OnDisable()
        {
            if (director != null)
            {
                // If this component is disabled while a cinematic is playing,
                // ensure player control is restored immediately.
                EnableControl(director);
                director.played -= DisableControl;
                director.stopped -= EnableControl;
            }
        }

        void DisableControl(PlayableDirector director)
        {
            actionScheduler?.CancelCurrentAction();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        void EnableControl(PlayableDirector director)
        {
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
}
