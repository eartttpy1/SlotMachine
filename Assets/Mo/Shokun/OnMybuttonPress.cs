using UnityEngine;

public class OnMybuttonPress : MonoBehaviour
{
    public AudioSource audioSource;
    public void GO()
    {
        AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
    }
}
