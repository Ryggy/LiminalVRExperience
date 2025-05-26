using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    [SerializeField] AudioSource audioInteract;
    [SerializeField] AudioSource audioAmbient;
    public AudioClip audioInteractClip;
    public AudioClip audioAmbientClip;
    [SerializeField] GameController gameController;
    bool shouldTransition = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(shouldTransition)
        {
            if (audioInteract.volume > 0)
            {
                audioInteract.volume -= 0.01f;
            }
            else
            {
                audioInteract.clip = audioInteractClip;
                audioAmbient.clip = audioAmbientClip;
                audioInteract.Play();
                audioAmbient.Play();
                shouldTransition = false;
            }
        }

        else
        {
            if(gameController.playerIsInteracting && audioInteract.volume < 1)
            {
                audioInteract.volume += 0.01f;
            }

            else if (!gameController.playerIsInteracting && audioInteract.volume > 0)
            {
                audioInteract.volume -= 0.01f;
            }
        }

    }

    public void UpdateAudioClips(AudioClip ambient, AudioClip interact)
    {
        audioAmbientClip = ambient;
        audioInteractClip = interact;
        shouldTransition = true;
    }



}
