using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.4f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private string startMusic;

    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private AudioClip[] sfxClips;

    private Dictionary<string, AudioClip> musicDictionary;
    private Dictionary<string, AudioClip> sfxDictionary;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Inicjalizacja słowników
        musicDictionary = new Dictionary<string, AudioClip>();
        sfxDictionary = new Dictionary<string, AudioClip>();

        // Dodanie klipów muzyki do słownika
        foreach (AudioClip clip in musicClips)
        {
            musicDictionary[clip.name] = clip;
        }

        // Dodanie klipów efektów dźwiękowych do słownika
        foreach (AudioClip clip in sfxClips)
        {
            sfxDictionary[clip.name] = clip;
        }
    }

    void Start()
    {
        /*if (masterVolumeSlider)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            masterVolumeSlider.value = gameData.masterVolume;
        }

        if (musicVolumeSlider)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            musicVolumeSlider.value = gameData.musicVolume;
        }

        if (sfxVolumeSlider)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
            sfxVolumeSlider.value = gameData.sfxVolume;
        }*/
        
        StartCoroutine(FadeIn(startMusic));
    }

    // Metoda odtwarzania muzyki po nazwie
    public void PlayMusic(string name, bool loop = true)
    {
        if (name == "") return;
        
        if (musicDictionary.TryGetValue(name, out AudioClip newClip))
        {
            StartCoroutine(FadeOutAndChangeTrack(newClip, loop));
        }
        else
        {
            Debug.LogWarning($"Music clip with name '{name}' not found!");
        }
    }

    public void PlayMusicWithoutFade(string name)
    {
        StartCoroutine(FadeOut());
        StartCoroutine(PlayMusicAfterDelay(name));
    }

    private IEnumerator FadeOutAndChangeTrack(AudioClip newClip, bool loop)
    {
        // Ściszaj aktualną muzykę
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(musicVolume * masterVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        // Ustaw nowy klip i rozpocznij odtwarzanie
        musicSource.clip = newClip;
        musicSource.Play();
        musicSource.loop = loop;
        
        // Podgłaśniaj nową muzykę
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, timer / fadeDuration);
            yield return null;
        }
    }
    
    public void PlayUISFX(AudioClip clip, float volume = 0.6f)
    {
        if (clip == null || uiSource == null) return;
        uiSource.pitch = Random.Range(0.96f, 1.04f);
        uiSource.PlayOneShot(clip, volume);
    }
    
    // Metoda odtwarzania efektu dźwiękowego po nazwie
    public void PlaySfx(string name)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX clip with name '{name}' not found!");
        }
    }
    
    public void PlaySfx(AudioClip clip, float volume)
    {
        // Tworzymy tymczasowy AudioSource
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.pitch = Random.Range(0.9f, 1.1f);
        tempSource.volume = volume * sfxVolume * masterVolume; // Kopiujemy ustawienia głośności
        tempSource.Play();

        // Usuwamy AudioSource po zakończeniu dźwięku
        Destroy(tempSource, clip.length / tempSource.pitch);
    }
    
    public void PlaySfxWithoutVariation(AudioClip clip, float volume)
    {
        // Tworzymy tymczasowy AudioSource
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume * sfxVolume * masterVolume; // Kopiujemy ustawienia głośności
        tempSource.Play();

        // Usuwamy AudioSource po zakończeniu dźwięku
        Destroy(tempSource, clip.length / tempSource.pitch);
    }

    private IEnumerator FadeIn(string name)
    {
        if (musicDictionary.TryGetValue(name, out AudioClip newClip))
        {
            musicSource.clip = newClip;
            musicSource.volume = 0f;
            musicSource.Play();
            
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, timer / fadeDuration);
                yield return null;
            }

            musicSource.volume = musicVolume * masterVolume;
        }
        else
        {
            Debug.LogWarning($"Music clip with name '{name}' not found!");
        }
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        float initialVolume = musicSource.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(initialVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume * masterVolume;
    }

    private IEnumerator PlayMusicAfterDelay(string name)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (musicDictionary.TryGetValue(name, out AudioClip newClip))
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.clip = newClip;
            tempSource.volume = musicVolume * masterVolume;
            tempSource.Play();
        }
    }

    public void StopMusic()
    {
        StartCoroutine(FadeOut());
    }

    /*public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        gameData.masterVolume = masterVolume;
        musicSource.volume = musicVolume * masterVolume;
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        gameData.musicVolume = musicVolume;
        musicSource.volume = musicVolume * masterVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        gameData.sfxVolume = sfxVolume;
    }*/
}