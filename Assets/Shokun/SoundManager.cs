using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Sword Sound Clips")]
    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip greatSwordSound;

    [Header("Environment Sound Clips")]
    [SerializeField] private AudioClip openChestSound;

    [Header("Player Hurt Sound Clips")]
    [SerializeField] private AudioClip getHitSound;          // เสียงได้รับบาดเจ็บปกติ
    [SerializeField] private AudioClip shieldHitSound;      // เสียงโดนตีแต่ติดเกราะ

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
}