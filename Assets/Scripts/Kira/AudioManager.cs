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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
