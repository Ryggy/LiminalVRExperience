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
    [SerializeField]public float experienceLength;
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

    public Color currentColour;
    private int experienceIndex = 3;
    private Color startColor;
    private Color endColor;

    [SerializeField]
    public List<ArtExperience> experiences = new List<ArtExperience>();

    public List<Renderer> emissives = new List<Renderer>();


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

        currentColour = Color.Lerp(startColor, endColor, (1/experiences[experienceIndex].experienceLength) * (float)playableDirector.time);
        RenderSettings.fogColor = currentColour;

        foreach(Renderer renderer in emissives)
        {
            //renderer.material.color = currentColour;
            renderer.material.SetColor("_EmissionColor", currentColour);
        }
    }

    public void EnterNextExperience()
    {
        Debug.Log("boop");
        experienceIndex--;
        if(experienceIndex < 0) {experienceIndex = 2;}
        playableDirector.time = 0;
        playableDirector.playableAsset = experiences[experienceIndex].timeline;
        audioManager.UpdateAudioClips(experiences[experienceIndex].audioAmbient, experiences[experienceIndex].audioInteract);
        startColor = experiences[experienceIndex].startColor;
        endColor = experiences[experienceIndex].endColor;
    }
}
