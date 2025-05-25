using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurrentExperience
{
    intro,
    fire,
    water,
    earth,
    air,
    end
}

public class GameController : MonoBehaviour
{
    public bool playerIsInteracting;
    public CurrentExperience currentExperience;

    private bool canIntro = true;
    private int experienceIndex = 0;

    //call this when the player starts or stops interacting with the flowfield
    public void PlayerInteract()
    {
        playerIsInteracting = !playerIsInteracting;
    }

    void Update()
    {
        if(playerIsInteracting && currentExperience == CurrentExperience.intro)
        {
            canIntro = false;
            //start the timeline
        }
    }

    void EnterNextExperience()
    {
        
    }
}
