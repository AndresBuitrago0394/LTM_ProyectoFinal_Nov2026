using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class GunnerLightning : MonoBehaviour {

        public Transform end;

        public float updateInterval = 0.5f;

        public int pointCount = 10;
        public float randomOffset = 0.5f;
        
        // Added modern FMOD EventReference for the lightning audio
        [Header("Audio")]
        public FMODUnity.EventReference lightningAudioEvent;

        Transform[] m_Branch;
        float m_UpdateTime = 0;
        Vector3[] m_Points;
        LineRenderer m_LineRenderer;
        
        // Track the audio instance so it doesn't duplicate endlessly
        private FMOD.Studio.EventInstance lightningAudioInstance;

        // Use this for initialization
        void Start () {
            m_LineRenderer = GetComponent<LineRenderer> ();
            m_Points = new Vector3[pointCount];
            m_LineRenderer.positionCount = pointCount;
            m_LineRenderer.useWorldSpace = false;
        }
        
        // Use OnEnable to trigger the sound exactly when the prefab is spawned or activated
        void OnEnable()
        {
            if (!lightningAudioEvent.IsNull)
            {
                lightningAudioInstance = FMODUnity.RuntimeManager.CreateInstance(lightningAudioEvent);
                
                // Ensure the sound plays at the lightning's physical location
                lightningAudioInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
                
                lightningAudioInstance.start();
                
                // Release it so FMOD destroys the instance automatically once it finishes playing
                lightningAudioInstance.release();
            }
        }

        void Update () {

            if (Time.time >= m_UpdateTime)
            {
                m_LineRenderer.positionCount = pointCount;

                m_Points [0] = Vector3.zero;
                Vector3 Segment = (end.position - transform.position) / (pointCount - 1);

                for(int i = 1; i < pointCount-1; i++){
                    m_Points [i] = Segment * i;
                    m_Points [i].y += Random.Range (-randomOffset, randomOffset);
                    m_Points [i].x += Random.Range (-randomOffset, randomOffset);
                }

                m_Points [pointCount - 1] = end.position - transform.position;
                m_LineRenderer.SetPositions (m_Points);

                m_UpdateTime += updateInterval;
            }
        }
        
        // Stop the sound immediately if the lightning prefab is destroyed early
        void OnDestroy()
        {
            if (lightningAudioInstance.isValid())
            {
                lightningAudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }
}