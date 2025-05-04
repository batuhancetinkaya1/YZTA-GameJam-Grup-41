using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AudioClipData
{
    public string clipName;
    public AudioClip clip;
    public float volume = 1f;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public List<AudioClipData> audioClips;

    private Dictionary<string, AudioClipData> m_clipDict;

    // Playlist coroutine referansý
    private Coroutine m_playlistRoutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);

        m_clipDict = new Dictionary<string, AudioClipData>();
        foreach (var clipData in audioClips)
        {
            if (!m_clipDict.ContainsKey(clipData.clipName))
                m_clipDict.Add(clipData.clipName, clipData);
        }
    }

    /// <summary>
    /// PlayerPrefs'ten MasterVolume'u çeken basit bir property.
    /// Ýsterseniz bu deðeri bir slider ile set edebilirsiniz.
    /// Örn: PlayerPrefs.SetFloat("MasterVolume", newValue);
    /// </summary>
    private float MasterVolume => PlayerPrefs.GetFloat("MasterVolume", 1f);

    #region Tekil Müzik Çalma
    public void PlayMusic(string clipName, bool restart = false)
    {
        if (!m_clipDict.ContainsKey(clipName))
        {
            Debug.LogWarning("Audio clip not found: " + clipName);
            return;
        }

        AudioClipData data = m_clipDict[clipName];

        // Eðer ayný parça çalýyorsa ve restart istenmiyorsa, tekrar baþlatma.
        if (musicSource.isPlaying && musicSource.clip == data.clip && !restart)
            return;

        musicSource.clip = data.clip;

        // MasterVolume’u her seferinde çarparak kullanýyoruz
        musicSource.volume = data.volume * MasterVolume;
        musicSource.loop = data.loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
    #endregion

    #region Playlist Sistemi
    /// <summary>
    /// Verilen klip isimlerini sýrayla çalar, en son klip bitince baþa döner.
    /// </summary>
    public void PlaySequentialMusic(params string[] clipNames)
    {
        // Eðer daha önce bir playlist çalýnýyorsa durdur.
        if (m_playlistRoutine != null)
        {
            StopCoroutine(m_playlistRoutine);
            m_playlistRoutine = null;
        }

        // Yeni playlist coroutine'i baþlat
        m_playlistRoutine = StartCoroutine(PlaySequentialRoutine(clipNames));
    }

    /// <summary>
    /// Playlist'i durdurur (ve eðer çalýyorsa müzikSource da durdurur).
    /// </summary>
    public void StopPlaylist()
    {
        if (m_playlistRoutine != null)
        {
            StopCoroutine(m_playlistRoutine);
            m_playlistRoutine = null;
        }
        StopMusic();
    }

    /// <summary>
    /// Playlist mantýðý: klipleri sýrayla çal, bitince bir sonraki klibe geç.
    /// Bütün klipler bitince baþa dön.
    /// </summary>
    private IEnumerator PlaySequentialRoutine(string[] clipNames)
    {
        while (true) // Sürekli döngü
        {
            foreach (string clipName in clipNames)
            {
                if (!m_clipDict.ContainsKey(clipName))
                {
                    Debug.LogWarning("Audio clip not found in playlist: " + clipName);
                    continue;
                }

                AudioClipData data = m_clipDict[clipName];

                // Playlist mantýðýnda her bir klipte loop kapalý çalýnýr.
                musicSource.loop = false;

                // Volume ayarýný her parça baþýnda tekrar yapýyoruz
                musicSource.clip = data.clip;
                musicSource.volume = data.volume * MasterVolume;
                musicSource.Play();

                // Bu klip bitene kadar bekle
                yield return new WaitWhile(() => musicSource.isPlaying);
            }
            // Tüm liste bittiðinde while(true) sayesinde baþa döner.
        }
    }
    #endregion

    #region SFX Çalma
    public void PlaySFX(string clipName)
    {
        if (!m_clipDict.ContainsKey(clipName))
        {
            Debug.LogWarning("SFX clip not found: " + clipName);
            return;
        }

        AudioClipData data = m_clipDict[clipName];

        // OneShot ile çalarken de MasterVolume ile çarpýyoruz
        sfxSource.PlayOneShot(data.clip, data.volume * MasterVolume);
    }

    public void PlaySFXWithNewSource(string clipName, Vector3 position)
    {
        if (!m_clipDict.ContainsKey(clipName))
        {
            Debug.LogWarning("SFX clip not found: " + clipName);
            return;
        }

        AudioClipData data = m_clipDict[clipName];

        AudioSource tempSource = new GameObject("TempAudio").AddComponent<AudioSource>();
        tempSource.transform.position = position;
        tempSource.clip = data.clip;
        tempSource.volume = data.volume * MasterVolume;
        tempSource.loop = data.loop;
        tempSource.Play();

        // Klip süresi bitince geçici objeyi yok et
        Destroy(tempSource.gameObject, data.clip.length);
    }
    #endregion

    #region Volume Kontrol
    /// <summary>
    /// Bu method, müzik volümünü *direkt* ayarlar.
    /// Dilerseniz MasterVolume * MusicVolume þeklinde çarpma yapabilirsiniz.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
            Debug.Log($"Music volume set to {volume}");
        }
        else
        {
            Debug.LogWarning("MusicSource is null.");
        }
    }

    /// <summary>
    /// Bu method, sfx volümünü *direkt* ayarlar.
    /// Dilerseniz MasterVolume * SFXVolume þeklinde çarpma yapabilirsiniz.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
            Debug.Log($"SFX volume set to {volume}");
        }
        else
        {
            Debug.LogWarning("SFXSource is null.");
        }
    }
    #endregion
}
