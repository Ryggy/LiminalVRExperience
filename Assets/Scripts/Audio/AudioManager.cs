using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] bool playerIsInteracting;
    [SerializeField] AudioSource audioInteract;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playerIsInteracting && audioInteract.volume < 1)
        {
            audioInteract.volume += 0.01f;
        }

        else if (!playerIsInteracting && audioInteract.volume > 0)
        {
            audioInteract.volume -= 0.01f;
        }
    }

    //call this when the player starts or stops interacting with the flowfield
    public void PlayerInteract()
    {
        playerIsInteracting = !playerIsInteracting;
    }


}
