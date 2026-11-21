using UnityEngine;

namespace Gamekit2D
{
    public class FmodPlayer : MonoBehaviour
    {
        private float distance = 0.05f;
        private string Material = "Earth";
        private bool wasGrounded = true;
        private LayerMask LM = 1 << 31;
        
        private FMOD.Studio.EventInstance Footsteps;
        private PlayerCharacter pc;
       public FMOD.Studio.EventInstance Falling;
        FMOD.Studio.PLAYBACK_STATE PbState;
        [HideInInspector]
        public bool playerSubmerged = false;


        void Start()
        {
            Footsteps = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ellen/Ellen_Footsteps 2");
            Falling = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ellen/Ellen_Falling");
            pc = GetComponent<PlayerCharacter>();
        }

        void PlaySound(string path)
        {
            FMODUnity.RuntimeManager.PlayOneShot(path, transform.position);
        }

        void PlayMeleeEvent(string path)
        {
            FMODUnity.RuntimeManager.PlayOneShot(path, transform.position);
        }

        void FixedUpdate()
        {
            MaterialCheck();
            Debug.DrawRay(transform.position, Vector2.down * distance, Color.blue);

            PlayerLanded();
            wasGrounded = IsGrounded();

            Footsteps.setParameterByNameWithLabel("Material", Material);

            PlayerFalling();
        }

        void MaterialCheck()
        {
            RaycastHit2D hit;
            hit = Physics2D.Raycast(transform.position, Vector2.down, distance, LM);

            if (hit.collider)
            {
                if (hit.collider.tag == "Material: Earth")
                    Material = "Earth";
                else if (hit.collider.tag == "Material: Stone")
                    Material = "Stone";
                else
                    Material = "Earth";
            }
        }

        void PlayFootstepsEvent()
        {
            Footsteps.start();
        }

        void OnDestroy()
        {
            Footsteps.release();
            Falling.release();
        }

        void PlayerFalling()
        {
            Falling.getPlaybackState(out PbState);
            
            // FIX: Added 'pc != null' check to prevent NullReferenceException
            if (!IsGrounded() && pc.m_MoveVector.y < -3 && !playerSubmerged)
            {
                Falling.getPlaybackState(out PbState);
            if (!IsGrounded() && pc.m_MoveVector.y < -3 && !playerSubmerged)
            {
                // Falling.setParameterValue("Velocity", pc.m_MoveVector.y);   < ----Use this if you're using A version of FMOD before V2.00.00
                Falling.setParameterByName("Velocity", pc.m_MoveVector.y, false);
                if (PbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    Falling = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ellen/Ellen_Falling");
                    Falling.start();
                    Falling.release();
                }
            }
            else if ((IsGrounded() || playerSubmerged) && PbState != FMOD.Studio.PLAYBACK_STATE.STOPPED)
                Falling.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }

        }

       void PlayerLanded()
        {
            if (IsGrounded() && !wasGrounded && pc.m_MoveVector.y < 0)
            {
                FMOD.Studio.EventInstance Landing = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ellen/Ellen_Land");
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(Landing, transform, GetComponent<Rigidbody2D>());
                //Landing.setParameterByName("Velocity", pc.m_MoveVector.y);   /*< ----Use this if you're using A version of FMOD before V2.00.00*/
                Landing.setParameterByName("Velocity", pc.m_MoveVector.y, false);
                //Footsteps.setParameterByName("FootstepDuck", pc.m_MoveVector.y);   /*< ----Use this if you're using A version of FMOD before V2.00.00*/
                Footsteps.setParameterByName("FootstepDuck", pc.m_MoveVector.y, false);
                Landing.start();
                Landing.release();
                //Falling.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        bool IsGrounded()
        {
            return Physics2D.Raycast(transform.position, Vector2.down, distance, LM);
        }
    }
}