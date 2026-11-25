using UnityEngine;
using Gamekit2D;

public class FMODMusicLv4 : MonoBehaviour
{
    private static FMOD.Studio.EventInstance Music; //An event instance variable.

    private float Distance; //A variable which will hold the distance between the player and the goal that we set for them.
    private Vector3 GoalLocation; //A variable for holding co-ordinates of the goal our player is trying to reach.
    public GameObject Goal;  //A reference which we'll use to reference the goal, and through that we'll get the goals co-ordinates.
    private float progressLevel;  //Holds a value to represent how many "progress switches" the player has hit.

    
    void Start()
    {
        Music = FMODUnity.RuntimeManager.CreateInstance("event:/Music/Lv4 Music");
        Music.start();
        Music.release();

        //GoalLocation = Goal.transform.position;
    }

    public void Progress (float ProgressLevel)
    {
        Music.setParameterByName("Progress", ProgressLevel);
        progressLevel = ProgressLevel;
    }

    private void Update()
    {
        if (progressLevel == 4)
        {
            Distance = Vector3.Distance(PlayerCharacter.PlayerInstance.transform.position, GoalLocation);
            Music.setParameterByName("DistanceToGoal", Distance / 4);
        }
    }

    private void OnDestroy()
    {
        Music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }




    //The section below is used to make sure that only one instance of this class is avaiable in the scene for the Enemy Audio...
    //...Class to access. It then declares a public function called "ChangeThreatLevel" that the Enemy Audio Class can trigger...
    //...whenever an enemy is close enough to the player in order to make our music switch to the tense track.




    public static FMODMusicLv4 Instance { get; private set; } //A public reference variable of the same type as our class.
    // It can be accessed by any other classes or scripts in our game, but can only be set by this class we're in now.

    private int NumberOfEnemiesAttackingPlayer; //A variable which will hold the amount of enemies that are close to the player.

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ChangeThreatLevel(bool AttackingPlayer)
    {
        if (AttackingPlayer)
            NumberOfEnemiesAttackingPlayer++;
        else
            NumberOfEnemiesAttackingPlayer--;

        switch (NumberOfEnemiesAttackingPlayer)
        {
            case 1:
                Music.setParameterByName("Threat Level", 1);
                break;
            case 0:
                Music.setParameterByName("Threat Level", 0);
                break;
        }
        
    }

}