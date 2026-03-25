using System.Collections;
using UnityEngine;

public class MusicSingleton : MonoBehaviour
{
    public static MusicSingleton Instance {get; private set;}
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip ambientMusic;
    [SerializeField] private AudioClip ChaseMusic;

    [SerializeField] private float fadeInTime;
    [SerializeField] private float fadeOutTime;

    private Coroutine fadeRoutine;
    private bool isPlayingAmbient = false;
    private bool isPlayingChase = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void PlayAmbient()
    {
        if (!isPlayingAmbient)
        {
            PlayMusic(ambientMusic);
            isPlayingAmbient = true;
            isPlayingChase = false;
        }
    }

    public void PlayChase()
    {
        if (!isPlayingChase)
        {
            PlayMusic(ChaseMusic);
            isPlayingChase = true;
            isPlayingAmbient = false;
        }
    }

    void PlayMusic(AudioClip clip)
    {
        if(fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        StartCoroutine(FadeToClip(clip));
    }

    IEnumerator FadeToClip(AudioClip newClip)
    {
        // Fade out
        float startVolume = audioSrc.volume;

        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            audioSrc.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutTime);
            yield return null;
        }

        audioSrc.volume = 0f;

        // Switch clip
        audioSrc.clip = newClip;
        audioSrc.Play();

        // Fade in
        t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            audioSrc.volume = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }

        audioSrc.volume = 1f;
    }
}
