using UnityEngine;
using Gamekit2D;

public class EnemyAudio : MonoBehaviour
{
    //This section plays footsteps whenever the enemy takes a step.

    FMOD.Studio.EventInstance Event;
    
    void PlayEvent(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(path, gameObject);
    }

    void PlayAttackEvent(string path)
    {
        Event = FMODUnity.RuntimeManager.CreateInstance(path);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(Event, transform, GetComponent<Rigidbody2D>());
        Event.start();
        Event.release();
    }

    void StopAttackEvent()
    {
        Event.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    //This section checks the distance between an instance of an enemy and the player.

    private float EngagePlayerDistance = 7;
    private float ForgetPlayerDistance = 10;
    private bool NearPlayer;
    private bool prevNearPlayer;
    private Damageable Damage;
    private bool ThisEnemyDied;

    private void Start()
    {
        Damage = gameObject.GetComponent<Damageable>();
    }
    
    private void Update() 
    {
        // FIX: Prevent crash if the enemy is missing the Damageable component
        if (Damage == null) return;

        if (Damage.CurrentHealth > 0)
        {
            // FIX: Prevent crash if the player is dead or hasn't loaded into the scene yet
            if (PlayerCharacter.PlayerInstance != null)
            {
                if (Vector3.Distance(transform.position, PlayerCharacter.PlayerInstance.transform.position) < EngagePlayerDistance)
                {
                    NearPlayer = true;
                    if (prevNearPlayer != NearPlayer)
                    {
                        // FIX: Prevent crash if the FMODMusicLv4 manager is missing from the scene
                        if (FMODMusicLv4.Instance != null)
                            FMODMusicLv4.Instance.ChangeThreatLevel(NearPlayer);
                            
                        Debug.Log("change music threat level");
                    }
                }
                else if (Vector3.Distance(transform.position, PlayerCharacter.PlayerInstance.transform.position) > ForgetPlayerDistance)
                {
                    NearPlayer = false;
                    if (prevNearPlayer != NearPlayer)
                    {
                        if (FMODMusicLv4.Instance != null)
                            FMODMusicLv4.Instance.ChangeThreatLevel(NearPlayer);
                    }
                }
            }
        }
        else
        {
            if(NearPlayer && !ThisEnemyDied)
            {
                if (FMODMusicLv4.Instance != null)
                    FMODMusicLv4.Instance.ChangeThreatLevel(false);
                    
                ThisEnemyDied = true;
            }
        }

        prevNearPlayer = NearPlayer;
    }
}