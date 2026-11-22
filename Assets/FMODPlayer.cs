using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Gamekit2D
{
    public class FmodPlayer : MonoBehaviour
    {
        private float distance = 0.05f;
        
        // Changed to string to support the "Earth" and "Stone" labels
        private string Material;
        
        private bool wasGrounded = true;
        private LayerMask LM = 1 << 31;
        
        private FMOD.Studio.EventInstance Footsteps;
        
        private PlayerCharacter pc;

        public static FMOD.Studio.EventInstance Falling;
        FMOD.Studio.PLAYBACK_STATE PbState;
        [HideInInspector]
        public static bool playerSubmerged = false;

        void Start()
        {
            Footsteps = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ellen/Ellen_Footsteps 2");
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(Footsteps, transform, GetComponent<Rigidbody2D>());
            pc = GetComponent<PlayerCharacter>();
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
            IsGrounded();

            PlayerFalling();
        }

        void MaterialCheck()
        {
            RaycastHit2D hit;
            hit = Physics2D.Raycast(transform.position, Vector2.down, distance, LM);

            if (hit.collider)
            {
                // Updated to string labels
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
            // Updated to setParameterByNameWithLabel for strings
            Footsteps.setParameterByNameWithLabel("Material", Material);
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
            if (!IsGrounded() && pc.m_MoveVector.y < -3 && !playerSubmerged)
            {
                Falling.setParameterByName("Velocity", pc.m_MoveVector.y);
                if (PbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    Falling = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ellen/Ellen_Falling");
                    Falling.start();
                    Falling.release();
                }
            }
            else if ((IsGrounded() || playerSubmerged) && PbState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
                Falling.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        void PlayerLanded()
        {
            if (IsGrounded() && !wasGrounded && pc.m_MoveVector.y < 0)
            {
                FMOD.Studio.EventInstance Landing = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ellen/Ellen_Land");
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(Landing, transform, GetComponent<Rigidbody2D>());
                Landing.setParameterByName("Velocity", pc.m_MoveVector.y);
                Footsteps.setParameterByName("FootstepDuck", pc.m_MoveVector.y);
                Landing.start();
                Landing.release();
                Falling.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        bool IsGrounded()
        {
            return Physics2D.Raycast(transform.position, Vector2.down, distance, LM);
        }
    }
}