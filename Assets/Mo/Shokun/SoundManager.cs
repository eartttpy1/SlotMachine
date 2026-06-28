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
    [SerializeField] private AudioClip shieldHitSound;

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayDefaultBGM();
    }

    public void PlaySwordSound()
    {
        if (sfxSource != null && swordSound != null)
        {
            sfxSource.PlayOneShot(swordSound);
        }
    }

    public void PlayGreatSwordSound()
    {
        if (sfxSource != null && greatSwordSound != null)
        {
            sfxSource.PlayOneShot(greatSwordSound);
        }
    }

    public void PlayOpenChestSound()
    {
        if (sfxSource != null && openChestSound != null)
        {
            sfxSource.PlayOneShot(openChestSound);
        }
    }

    public void PlayGetHitSound()
    {
        if (sfxSource != null && getHitSound != null)
        {
            sfxSource.PlayOneShot(getHitSound);
        }
    }

    public void PlayShieldHitSound()
    {
        if (sfxSource != null && shieldHitSound != null)
        {
            sfxSource.PlayOneShot(shieldHitSound);
        }
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayButtonHover()
    {
        if (sfxSource != null && buttonHoverSound != null)
        {
            sfxSource.PlayOneShot(buttonHoverSound);
        }
    }

    public void PlayHealSound()
    {
        if (sfxSource != null && healSound != null)
        {
            sfxSource.PlayOneShot(healSound);
        }
    }

    public void PlaySlotSpinSound()
    {
        if (sfxSource != null && slotSpinSound != null)
        {
            sfxSource.PlayOneShot(slotSpinSound);
        }
    }

    public void PlayDefaultBGM()
    {
        if (bgmMusic != null)
        {
            PlayBGM(bgmMusic);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource != null && clip != null)
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }
}