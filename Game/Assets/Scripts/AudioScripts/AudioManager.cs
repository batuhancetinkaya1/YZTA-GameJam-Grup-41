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

    // Playlist coroutine referans�
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
    /// PlayerPrefs'ten MasterVolume'u �eken basit bir property.
    /// �sterseniz bu de�eri bir slider ile set edebilirsiniz.
    /// �rn: PlayerPrefs.SetFloat("MasterVolume", newValue);
    /// </summary>
    private float MasterVolume => PlayerPrefs.GetFloat("MasterVolume", 1f);

    #region Tekil M�zik �alma
    public void PlayMusic(string clipName, bool restart = false)
    {
        if (!m_clipDict.ContainsKey(clipName))
        {
            Debug.LogWarning("Audio clip not found: " + clipName);
            return;
        }

        AudioClipData data = m_clipDict[clipName];

        // E�er ayn� par�a �al�yorsa ve restart istenmiyorsa, tekrar ba�latma.
        if (musicSource.isPlaying && musicSource.clip == data.clip && !restart)
            return;

        musicSource.clip = data.clip;

        // MasterVolume�u her seferinde �arparak kullan�yoruz
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
    /// Verilen klip isimlerini s�rayla �alar, en son klip bitince ba�a d�ner.
    /// </summary>
    public void PlaySequentialMusic(params string[] clipNames)
    {
        // E�er daha �nce bir playlist �al�n�yorsa durdur.
        if (m_playlistRoutine != null)
        {
            StopCoroutine(m_playlistRoutine);
            m_playlistRoutine = null;
        }

        // Yeni playlist coroutine'i ba�lat
        m_playlistRoutine = StartCoroutine(PlaySequentialRoutine(clipNames));
    }

    /// <summary>
    /// Playlist'i durdurur (ve e�er �al�yorsa m�zikSource da durdurur).
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
    /// Playlist mant���: klipleri s�rayla �al, bitince bir sonraki klibe ge�.
    /// B�t�n klipler bitince ba�a d�n.
    /// </summary>
    private IEnumerator PlaySequentialRoutine(string[] clipNames)
    {
        while (true) // S�rekli d�ng�
        {
            foreach (string clipName in clipNames)
            {
                if (!m_clipDict.ContainsKey(clipName))
                {
                    Debug.LogWarning("Audio clip not found in playlist: " + clipName);
                    continue;
                }

                AudioClipData data = m_clipDict[clipName];

                // Playlist mant���nda her bir klipte loop kapal� �al�n�r.
                musicSource.loop = false;

                // Volume ayar�n� her par�a ba��nda tekrar yap�yoruz
                musicSource.clip = data.clip;
                musicSource.volume = data.volume * MasterVolume;
                musicSource.Play();

                // Bu klip bitene kadar bekle
                yield return new WaitWhile(() => musicSource.isPlaying);
            }
            // T�m liste bitti�inde while(true) sayesinde ba�a d�ner.
        }
    }
    #endregion

    #region SFX �alma
    public void PlaySFX(string clipName)
    {
        if (!m_clipDict.ContainsKey(clipName))
        {
            Debug.LogWarning("SFX clip not found: " + clipName);
            return;
        }

        AudioClipData data = m_clipDict[clipName];

        // OneShot ile �alarken de MasterVolume ile �arp�yoruz
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

        // Klip s�resi bitince ge�ici objeyi yok et
        Destroy(tempSource.gameObject, data.clip.length);
    }
    #endregion

    #region Volume Kontrol
    /// <summary>
    /// Bu method, m�zik vol�m�n� *direkt* ayarlar.
    /// Dilerseniz MasterVolume * MusicVolume �eklinde �arpma yapabilirsiniz.
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
    /// Bu method, sfx vol�m�n� *direkt* ayarlar.
    /// Dilerseniz MasterVolume * SFXVolume �eklinde �arpma yapabilirsiniz.
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
