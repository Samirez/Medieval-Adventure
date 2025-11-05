using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using RPG.Resources;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {

        [SerializeField] float speed = 1;
        [SerializeField] bool isHoming = false;
        [SerializeField] GameObject hitEffect = null;
        [SerializeField] float maxLifeTime = 10;
        [SerializeField] float lifeAfterImpact = 2;
        [SerializeField] GameObject[] destroyOnCollision = null;

        Health target = null;
        float damage = 0;

        private void Start()
        {
            transform.LookAt(GetAimLocation());
        }

        void Update()
        {
            if (target == null) return;
            if (isHoming && !target.IsDead())
            {
                transform.LookAt(GetAimLocation());
            }
        
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        
        public void SetTarget(Health target, float damage)
        {
            this.target = target;
            this.damage = damage;

            Destroy(gameObject, maxLifeTime);
        }

        private Vector3 GetAimLocation()
        {
            CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();

            if (targetCapsule == null)
            {
                return target.transform.position;
            }

            return target.transform.position + Vector3.up * targetCapsule.height / 2;
        }
        
        public void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != target) return; 
            if (target.IsDead()) return;
            target.TakeDamage(damage);

            speed = 0;

            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, GetAimLocation(), transform.rotation);
                Destroy(effect, lifeAfterImpact);
            }
            
            foreach (GameObject toDestroy in destroyOnCollision)
            {
                if (toDestroy.scene.IsValid())
                {
                    Destroy(toDestroy);
                }
            }
        }
    }
}

