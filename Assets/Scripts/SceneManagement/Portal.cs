using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;

namespace RPG.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A, B, C, D, E
        }

        [Header("Portal Settings")]
        [SerializeField] int sceneToLoad = -1;
        [SerializeField] Transform spawnPoint;
        [SerializeField] DestinationIdentifier destination;
        [SerializeField] float fadeOutTime = 2f;
        [SerializeField] float fadeInTime = 1f;
        [SerializeField] float fadeWaitTime = 1f;

        [System.Obsolete]
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player entered portal trigger.");
                StartCoroutine(Transition());
            }
        }

        [System.Obsolete]
        private IEnumerator Transition()
        {
            if (sceneToLoad < 0 || sceneToLoad >= SceneManager.sceneCountInBuildSettings)
            {
                yield break;
            }

            DontDestroyOnLoad(gameObject);

            Fader fader = FindFirstObjectByType<Fader>();
            yield return fader.FadeOut(fadeOutTime);
            SavingWrapper wrapper = FindFirstObjectByType<SavingWrapper>();
            wrapper.Save();
            // save current level
            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            wrapper.Load();
            Portal otherPortal = GetOtherPortal();

            UpdatePlayer(otherPortal);

            wrapper.Save();
            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);
            Destroy(gameObject);
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            if (otherPortal == null)
            {
                return;
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                return;
            }

            NavMeshAgent navMeshAgent = player.GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                return;
            }
            navMeshAgent.enabled = false;
            player.transform.position = otherPortal.spawnPoint.position;
            player.transform.rotation = otherPortal.spawnPoint.rotation;
            navMeshAgent.enabled = true;
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsByType<Portal>(FindObjectsSortMode.None))
            {
                if (portal == this) continue;
                if (portal.destination != destination) continue;

                Debug.Log("Matching portal found: " + portal.name);
                return portal;
            }

            Debug.LogError("Matching portal not found!");
            return null;
        }
    }
}