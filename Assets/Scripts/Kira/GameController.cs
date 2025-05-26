using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public enum CurrentExperience
{
    intro,
    fire,
    water,
    earth,
    air,
    end
}

[System.Serializable]
public struct ArtExperience
{
    [SerializeField]public CurrentExperience experience;
    [SerializeField]public PlayableAsset timeline;
    [SerializeField]public Color startColor;
    [SerializeField]public Color endColor;
    [SerializeField]public AudioClip audioAmbient;
    [SerializeField]public AudioClip audioInteract;
}

public class GameController : MonoBehaviour
{
    public bool playerIsInteracting;
    public PlayableDirector playableDirector;
    public AudioManager audioManager;

    private int experienceIndex = -1;
    private Color startColor;
    private Color endColor;

    [SerializeField]
    public List<ArtExperience> experiences = new List<ArtExperience>();


    //call this when the player starts or stops interacting with the flowfield

    void Start()
    {
        EnterNextExperience();
    }

    void Update()
    {
        if(playerIsInteracting)
        {
                playableDirector.time += Time.deltaTime;
        }
        playableDirector.Evaluate();
    }

    public void EnterNextExperience()
    {
        experienceIndex++;
        if(experienceIndex > experiences.Count-1) {experienceIndex = 0;}
        playableDirector.time = 0;
        playableDirector.playableAsset = experiences[experienceIndex].timeline;
        audioManager.audioAmbientClip = experiences[experienceIndex].audioAmbient;
        audioManager.audioInteractClip = experiences[experienceIndex].audioInteract;
        startColor = experiences[experienceIndex].startColor;
        endColor = experiences[experienceIndex].endColor;
    }
}
