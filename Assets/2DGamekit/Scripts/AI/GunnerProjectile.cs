using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class GunnerProjectile : MonoBehaviour
    {
        public Vector2 initialForce;
        public float timer = 1;
        public float fuse = 0.01f;
        public GameObject explosion;
        public float explosionTimer = 3;
        new Rigidbody2D rigidbody;

        protected GameObject m_HitEffect;

        [Header("FMOD Events")]
        public FMODUnity.EventReference fireEvent;
        public FMODUnity.EventReference impactEvent;

        void OnEnable()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            Destroy(gameObject, timer);

            m_HitEffect = Instantiate(explosion);
            m_HitEffect.SetActive(false);
        }

        public void Destroy()
        {
            // 2. Play the impact sound in world space before the projectile is destroyed
            if (!impactEvent.IsNull)
            {
                FMODUnity.RuntimeManager.PlayOneShot(impactEvent, transform.position);
            }

            m_HitEffect.transform.position = transform.position;
            m_HitEffect.SetActive(true);
            GameObject.Destroy(m_HitEffect, explosionTimer);
            Destroy(gameObject); // Note: This calls Unity's Object.Destroy, destroying the GameObject
        }

        void Start()
        {
            // 1. Play the firing sound the moment the projectile is launched
            if (!fireEvent.IsNull)
            {
                FMODUnity.RuntimeManager.PlayOneShot(fireEvent, transform.position);
            }

            rigidbody.AddForce(initialForce);
        }
    }
}