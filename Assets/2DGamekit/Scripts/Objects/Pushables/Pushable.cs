using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Pushable : MonoBehaviour
    {
        private FMOD.Studio.EventInstance PushSound;
        private FMOD.Studio.EventInstance FallSound;
        FMOD.Studio.PLAYBACK_STATE PbState;

        static ContactPoint2D[] s_ContactPointBuffer = new ContactPoint2D[16];
        static Dictionary<Collider2D, Pushable> s_PushableCache = new Dictionary<Collider2D, Pushable> ();

        public Transform playerPushingRightPosition;
        public Transform playerPushingLeftPosition;
        public Transform pushablePosition;

        public AudioSource pushableAudioSource;
        public AudioClip startingPushClip;
        public AudioClip loopPushClip;
        public AudioClip endPushClip;

        public bool Grounded {  get { return m_Grounded; } }

        protected SpriteRenderer m_SpriteRenderer;
        protected Rigidbody2D m_Rigidbody2D;
        protected bool m_Grounded;
        Collider2D[] m_WaterColliders;

        void Awake ()
        {
            // Kept the string event paths as requested
            PushSound = FMODUnity.RuntimeManager.CreateInstance("event:/Interactables/Pushable");
            FallSound = FMODUnity.RuntimeManager.CreateInstance("event:/Interactables/Box_Falling");

            m_SpriteRenderer = GetComponent<SpriteRenderer>(); 
            m_Rigidbody2D = GetComponent<Rigidbody2D> ();

            if (s_PushableCache.Count == 0)
            {
                // Updated obsolete FindObjectsOfType to FindObjectsByType
                Pushable[] pushables = FindObjectsByType<Pushable>(FindObjectsSortMode.None);

                for (int i = 0; i < pushables.Length; i++)
                {
                    Collider2D[] pushableColliders = pushables[i].GetComponents<Collider2D> ();

                    for (int j = 0; j < pushableColliders.Length; j++)
                    {
                        s_PushableCache.Add (pushableColliders[j], pushables[i]);
                    }
                }
            }

            // Updated obsolete FindObjectsOfType to FindObjectsByType
            WaterArea[] waterAreas = FindObjectsByType<WaterArea>(FindObjectsSortMode.None);
            m_WaterColliders = new Collider2D[waterAreas.Length];
            for (int i = 0; i < waterAreas.Length; i++)
            {
                m_WaterColliders[i] = waterAreas[i].GetComponent<Collider2D> ();
            }
        }

        void FixedUpdate ()
        {
            FallSound.getPlaybackState(out PbState);

            Vector2 velocity = m_Rigidbody2D.linearVelocity;
            velocity.x = 0f;
            m_Rigidbody2D.linearVelocity = velocity;
            CheckGrounded();

            for (int i = 0; i < m_WaterColliders.Length; i++)
            {
                if (m_Rigidbody2D.IsTouching (m_WaterColliders[i]))
                {
                    m_Rigidbody2D.constraints |= RigidbodyConstraints2D.FreezePositionX;
                    if (PbState != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                        FallSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
                else
                {
                    if (!m_Grounded && velocity.y < -3)
                    {
                        // Updated obsolete setParameterValue to setParameterByName
                        FallSound.setParameterByName("Velocity", velocity.y);
                        if (PbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                        {
                            FMODUnity.RuntimeManager.AttachInstanceToGameObject(FallSound, transform, GetComponent<Rigidbody2D>());
                            FallSound.start();
                        }
                    }
                }
            }
        }

        private void OnDestroy()
        {
            PushSound.release();
            FallSound.release();
        }

        public void StartPushing()
        {
            pushableAudioSource.loop = false;
            pushableAudioSource.clip = startingPushClip;
            pushableAudioSource.Play();

            Debug.Log("START");
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(PushSound, transform, GetComponent<Rigidbody2D>());
            PushSound.start();
        }

        public void EndPushing()
        {
            pushableAudioSource.loop = false;
            pushableAudioSource.clip = endPushClip;
            pushableAudioSource.Play();

            Debug.Log("END");
            PushSound.keyOff();
        }

        public void Move (Vector2 movement)
        {
            m_Rigidbody2D.position = m_Rigidbody2D.position + movement;
            
            if (!pushableAudioSource.isPlaying)
            {
                pushableAudioSource.clip = loopPushClip;
                pushableAudioSource.loop = true;
                pushableAudioSource.Play();
            }
        }

        protected void CheckGrounded()
        {
            m_Grounded = false;

            int count = m_Rigidbody2D.GetContacts(s_ContactPointBuffer);
            for(int i = 0; i < count; ++i)
            {
                if(s_ContactPointBuffer[i].normal.y > 0.9f)
                {
                    m_Grounded = true;

                    Pushable pushable;
                
                    if(s_PushableCache.TryGetValue (s_ContactPointBuffer[i].collider, out pushable))
                    {
                        m_SpriteRenderer.sortingOrder = pushable.m_SpriteRenderer.sortingOrder + 1;
                    }
                }
            }
        }
    }
}