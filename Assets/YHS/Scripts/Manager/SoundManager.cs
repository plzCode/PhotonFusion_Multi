using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    SFX,
    BGM
}

[System.Serializable]
public class SoundData
{
    public string soundName;
    public AudioClip clip;
    public SoundType type = SoundType.SFX;
    [Range(0f, 1f)] public float volume = 1f;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sound List")]
    [SerializeField] private List<SoundData> soundDataList;

    private Dictionary<string, SoundData> soundDict;

    private AudioSource bgmSource;
    private List<AudioSource> sfxSources = new List<AudioSource>();

    [Header("Max SFX Sources")]
    [SerializeField] private int sfxSourcePoolSize = 10;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        soundDict = new Dictionary<string, SoundData>();
        foreach (var sound in soundDataList)
        {
            if (!soundDict.ContainsKey(sound.soundName))
                soundDict.Add(sound.soundName, sound);
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        for (int i = 0; i < sfxSourcePoolSize; i++)
        {
            var sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.loop = false;
            sfxSources.Add(sfx);
        }
    }

    public void Play(string soundName)
    {
        if (!soundDict.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found.");
            return;
        }

        var data = soundDict[soundName];

        if (data.type == SoundType.BGM)
        {
            bgmSource.clip = data.clip;
            bgmSource.volume = data.volume;
            bgmSource.Play();
        }
        else
        {
            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.clip = data.clip;
                source.volume = data.volume;
                source.Play();
            }
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
                return source;
        }
        return null; // 풀 초과시 무시
    }
}