using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float xOffset = 0;
        [SerializeField] float yOffset = 5;
        [SerializeField] float zOffset = -10;
        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main.GetComponent<Camera>();
        }

        void Update()
        {
            Vector3 offset = new Vector3(xOffset, yOffset, zOffset);
            mainCamera.transform.position = target.position + offset;
            mainCamera.transform.LookAt(target);
        }
    }
}