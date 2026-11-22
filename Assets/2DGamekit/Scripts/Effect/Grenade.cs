using System.Collections;
using UnityEngine;

namespace Gamekit2D
{
    public class Grenade : MonoBehaviour
    {
        public Vector2 initialForce;
        public float timer = 1;
        public GameObject explosion;
        public float explosionTimer = 3;

        Rigidbody2D Rigidbody;
        
        // FMOD Events
        [Header("FMOD Events")]
        public FMODUnity.EventReference throwEvent;
        public FMODUnity.EventReference explodeEvent;

        void OnEnable()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        IEnumerator Start()
        {
            // 1. Play the "enter" or throw sound
            if (!throwEvent.IsNull)
            {
                FMODUnity.RuntimeManager.PlayOneShot(throwEvent, transform.position);
            }

            Rigidbody.AddForce(initialForce);
            yield return new WaitForSeconds(timer);
            
            // 2. Play the explosion sound right as the timer finishes
            if (!explodeEvent.IsNull)
            {
                FMODUnity.RuntimeManager.PlayOneShot(explodeEvent, transform.position);
            }

            var eGo = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(eGo, explosionTimer);
            Destroy(gameObject);
        }
    }
}