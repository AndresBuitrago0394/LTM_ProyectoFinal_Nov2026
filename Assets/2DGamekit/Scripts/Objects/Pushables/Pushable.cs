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
        private FMOD.Studio.EventInstance BobbleSound;
        FMOD.Studio.PLAYBACK_STATE PbState2;

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

        private float y;

        void Awake ()
        {
            PushSound = FMODUnity.RuntimeManager.CreateInstance("event:/Interactables/Pushable");
            FallSound = FMODUnity.RuntimeManager.CreateInstance("event:/Interactables/Box_Falling");
            BobbleSound = FMODUnity.RuntimeManager.CreateInstance("event:/Interactables/Bobbling Create");

            m_SpriteRenderer = GetComponent<SpriteRenderer>(); 
            m_Rigidbody2D = GetComponent<Rigidbody2D> ();

            if (s_PushableCache.Count == 0)
            {
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
            
            // FIX: Replaced 'transform' with 'gameObject' for modern API compliance
            BobbleSound.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject, GetComponent<Rigidbody2D>()));
            
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

                    BobbleSound.getPlaybackState(out PbState2);
                    if (PbState2 != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                        BobbleSound.start(); 

                    y = velocity.y;
                    if (y < 0)
                        y *= -1;
                    BobbleSound.setParameterByName("Bobble", y, false);
                }
                else
                {
                    if (!m_Grounded && velocity.y < -3)
                    {
                        FallSound.setParameterByName("Velocity", velocity.y);
                        if (PbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                        {
                            // FIX: Replaced 'transform' with 'gameObject' and explicitly set 3D attributes before starting
                            FMODUnity.RuntimeManager.AttachInstanceToGameObject(FallSound, gameObject, GetComponent<Rigidbody2D>());
                            FallSound.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject, GetComponent<Rigidbody2D>()));
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
            BobbleSound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            BobbleSound.release();
        }

        public void StartPushing()
        {
            pushableAudioSource.loop = false;
            pushableAudioSource.clip = startingPushClip;
            pushableAudioSource.Play();

            Debug.Log("START");
            
            // FIX: Replaced 'transform' with 'gameObject' and explicitly set 3D attributes before starting
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(PushSound, gameObject, GetComponent<Rigidbody2D>());
            PushSound.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject, GetComponent<Rigidbody2D>()));
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