using UnityEngine;
using System.Collections;

public class EffectShokun : MonoBehaviour
{
    public float duration = 1f; // Duration of the effect in seconds
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(EffectCoroutine());
    }

    IEnumerator EffectCoroutine()
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(duration);
        // Destroy the GameObject this script is attached to
        Destroy(gameObject);
    }
}
