using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour, IAudioFeedback
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioInteract;
    [SerializeField] private AudioSource audioAmbient;

    [Header("Current Clips")]
    [SerializeField] private AudioClip audioInteractClip;
    [SerializeField] private AudioClip audioAmbientClip;

    [Header("Pitch Settings")]
    [Range(0.8f, 1.2f)] public float minPitch = 0.95f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.05f;

    [SerializeField] private GameController gameController;

    private bool _shouldTransition = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        if (gameController == null)
        {
            Debug.LogWarning("AudioManager: GameController not assigned.");
        }
    }
    
    private void Update()
    {
        HandleVolumeTransition();
    }
   
    private void HandleVolumeTransition()
    {
        if (_shouldTransition)
        {
            FadeOutOldAndStartNewClips();
        }
        else
        {
            AdjustInteractVolumeBasedOnInteraction();
        }
    }
    
    private void FadeOutOldAndStartNewClips()
    {
        if (audioInteract == null || audioAmbient == null)
            return;

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
            _shouldTransition = false;
        }
    }
    
    private void AdjustInteractVolumeBasedOnInteraction()
    {
        if (audioInteract == null || gameController == null)
            return;

        if (gameController.playerIsInteracting && audioInteract.volume < 1f)
        {
            audioInteract.volume += 0.01f;
        }
        else if (!gameController.playerIsInteracting && audioInteract.volume > 0f)
        {
            audioInteract.volume -= 0.01f;
        }
    }
    
    public void PlayInteractSound()
    {
        Debug.Log("Playing Interact Sound");
        if (audioInteract == null || audioInteractClip == null) return;

        audioInteract.pitch = Random.Range(minPitch, maxPitch);
        audioInteract.PlayOneShot(audioInteractClip);
    }

    public void UpdateAudioClips(AudioClip ambient, AudioClip interact)
    {
        Instance.audioAmbientClip = ambient;
        Instance.audioInteractClip = interact;
        Instance._shouldTransition = true;
    }
}
