using UnityEngine;
using UnityEngine.Tilemaps;

namespace Gamekit2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Damager))]
    public class Bullet : MonoBehaviour
    {
        [FMODUnity.EventRef]
        public string ProjectileEvent;
        FMOD.Studio.EventInstance PE;
        FMOD.Studio.PLAYBACK_STATE PbState;

        [FMODUnity.EventRef]
        public string ImpactEvent;

        public bool destroyWhenOutOfView = true;
        public bool spriteOriginallyFacesLeft;

        [Tooltip("If -1 never auto destroy, otherwise bullet is return to pool when that time is reached")]
        public float timeBeforeAutodestruct = -1.0f;
    
        [HideInInspector]
        public BulletObject bulletPoolObject;
        [HideInInspector]
        public Camera mainCamera;

        protected SpriteRenderer m_SpriteRenderer;
        static readonly int VFX_HASH = VFXController.StringToHash("BulletImpact");

        const float k_OffScreenError = 0.01f;

        protected float m_Timer;

        private void OnEnable()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Timer = 0.0f;
            PE = FMODUnity.RuntimeManager.CreateInstance(ProjectileEvent);
            PE.getPlaybackState(out PbState);
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(PE, transform, GetComponent<Rigidbody2D>());
            PE.start(); 
            PE.release(); 
        }

        public void ReturnToPool ()
        {
            bulletPoolObject.ReturnToPool ();
        }

        void FixedUpdate ()
        {
            if (destroyWhenOutOfView)
            {
                Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > -k_OffScreenError &&
                                screenPoint.x < 1 + k_OffScreenError && screenPoint.y > -k_OffScreenError &&
                                screenPoint.y < 1 + k_OffScreenError;

                if (!onScreen && PbState != FMOD.Studio.PLAYBACK_STATE.PLAYING) 
                    bulletPoolObject.ReturnToPool();
            }

            if (timeBeforeAutodestruct > 0)
            {
                m_Timer += Time.deltaTime;
                if (m_Timer > timeBeforeAutodestruct)
                {
                    bulletPoolObject.ReturnToPool();
                    PE.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }
            }
        }

        public void OnSpitHitDamageable(Damager origin, Damageable damageable)
        {
            FindSurface(origin.LastHit);
            PE.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
            
            // Expanded to set the "Enemy" label for spit impacts
            FMOD.Studio.EventInstance SpitImpact = FMODUnity.RuntimeManager.CreateInstance(ImpactEvent);
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(SpitImpact, damageable.transform, damageable.GetComponent<Rigidbody2D>());
            SpitImpact.setParameterByNameWithLabel("BulletCollide", "Enemy");
            SpitImpact.start();
            SpitImpact.release();
        }

        public void OnBulletHitDamageable(Damager origin, Damageable damageable)
        {
            FindSurface(origin.LastHit);
            PE.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
            
            FMOD.Studio.EventInstance BulletImpact = FMODUnity.RuntimeManager.CreateInstance(ImpactEvent);
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(BulletImpact, damageable.transform, damageable.GetComponent<Rigidbody2D>());
            
            // CHANGED: Use setParameterByNameWithLabel for the "Enemy" string
            BulletImpact.setParameterByNameWithLabel("BulletCollide", damageable.name == "GunnerShield" ? "Shield" : "Object");
            
            BulletImpact.start();
            BulletImpact.release();
        }

        public void OnHitNonDamageable(Damager origin)
        {
            FindSurface(origin.LastHit);
            PE.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
            
            // CHANGED: Expanded from a PlayOneShot call to an instance call so we can pass the "Object" string parameter
            FMOD.Studio.EventInstance BulletImpact = FMODUnity.RuntimeManager.CreateInstance(ImpactEvent);
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(BulletImpact, transform, GetComponent<Rigidbody2D>());
            
            BulletImpact.setParameterByNameWithLabel("BulletCollide", "Object");
            
            BulletImpact.start();
            BulletImpact.release();
        }

        protected void FindSurface(Collider2D collider)
        {
            Vector3 forward = spriteOriginallyFacesLeft ? Vector3.left : Vector3.right;
            if (m_SpriteRenderer.flipX) forward.x = -forward.x;

            TileBase surfaceHit = PhysicsHelper.FindTileForOverride(collider, transform.position, forward);

            VFXController.Instance.Trigger(VFX_HASH, transform.position, 0, m_SpriteRenderer.flipX, null, surfaceHit);
        }
    }
}