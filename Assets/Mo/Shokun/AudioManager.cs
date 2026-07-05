using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip bgmMusic;

    [Header("Sword Sound Clips")]
    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip greatSwordSound;

    [Header("Environment Sound Clips")]
    [SerializeField] private AudioClip openChestSound;

    [Header("Player Hurt Sound Clips")]
    [SerializeField] private AudioClip getHitSound;
    [SerializeField] private AudioClip parrySound;

    [Header("UI Sound Clips")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip slotSpinSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        EnsureAudioSources();
        PlayDefaultBGM();
    }

    private void EnsureAudioSources()
    {
        // Unity objects that are destroyed or missing evaluate to null in C#
        if (sfxSource == null || bgmSource == null)
        {
            AudioSource[] sources = GetComponentsInChildren<AudioSource>(true);
            
            if (sfxSource == null)
            {
                sfxSource = System.Array.Find(sources, s => s != null && s.gameObject.name.ToLower().Contains("sfx"));
                if (sfxSource == null && sources.Length > 0) sfxSource = sources[0];
                if (sfxSource == null)
                {
                    GameObject sfxObj = new GameObject("SFXSource");
                    sfxObj.transform.SetParent(transform);
                    sfxSource = sfxObj.AddComponent<AudioSource>();
                }
            }
            
            if (bgmSource == null)
            {
                bgmSource = System.Array.Find(sources, s => s != null && (s.gameObject.name.ToLower().Contains("bgm") || s.gameObject.name.ToLower().Contains("music")));
                if (bgmSource == null && sources.Length > 1) bgmSource = sources[1];
                if (bgmSource == null && sources.Length > 0 && sfxSource != sources[0]) bgmSource = sources[0];
                if (bgmSource == null)
                {
                    GameObject bgmObj = new GameObject("BGMSource");
                    bgmObj.transform.SetParent(transform);
                    bgmSource = bgmObj.AddComponent<AudioSource>();
                }
            }
            if (bgmSource != null)
            {
                bgmSource.volume = 0.5f;
            }
        }
    }

    private void Start()
    {
        EnsureAudioSources();
        PlayDefaultBGM();
    }

    public void PlaySwordSound()
    {
        EnsureAudioSources();
        if (sfxSource != null && swordSound != null)
        {
            sfxSource.PlayOneShot(swordSound);
        }
    }

    public void PlayGreatSwordSound()
    {
        EnsureAudioSources();
        if (sfxSource != null && greatSwordSound != null)
        {
            sfxSource.PlayOneShot(greatSwordSound);
        }
    }

    public void PlayOpenChestSound()
    {
        EnsureAudioSources();
        if (sfxSource != null && openChestSound != null)
        {
            sfxSource.PlayOneShot(openChestSound);
        }
    }

    public void PlayGetHitSound()
    {
        EnsureAudioSources();
        if (sfxSource != null && getHitSound != null)
        {
            Debug.Log("AudioManager: Playing GetHitSound -> " + getHitSound.name);
            sfxSource.PlayOneShot(getHitSound);
        }
    }



    public void PlayButtonClick()
    {
        EnsureAudioSources();
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayButtonHover()
    {
        EnsureAudioSources();
        if (sfxSource != null && buttonHoverSound != null)
        {
            sfxSource.PlayOneShot(buttonHoverSound);
        }
    }

    public void PlayHealSound()
    {
        EnsureAudioSources();
        if (sfxSource != null && healSound != null)
        {
            sfxSource.PlayOneShot(healSound);
        }
    }

    public void PlaySlotSpinSound()
    {
        EnsureAudioSources();
        if (sfxSource != null && slotSpinSound != null)
        {
            sfxSource.PlayOneShot(slotSpinSound);
        }
    }

    public void PlayDefaultBGM()
    {
        EnsureAudioSources();
        if (bgmMusic != null)
        {
            PlayBGM(bgmMusic);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        EnsureAudioSources();
        if (bgmSource != null && clip != null)
        {
            bgmSource.volume = 0.5f;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        EnsureAudioSources();
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        EnsureAudioSources();
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }

    public void PlayParrySound()
    {
        EnsureAudioSources();
        if (sfxSource != null && parrySound != null)
        {
            Debug.Log("AudioManager: Playing ParrySound -> " + parrySound.name);
            sfxSource.PlayOneShot(parrySound);
        }
    }
}