using UnityEngine;

/// <summary>
/// Handles sound effects for the player.
/// Assign clips in the Inspector.
/// Call Die() from PlayerController to also trigger a sound.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip deathClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayJump()
    {
        PlayClip(jumpClip);
    }

    public void PlayDash()
    {
        PlayClip(dashClip);
    }

    public void PlayDeath()
    {
        if (deathClip != null)
            AudioSource.PlayClipAtPoint(deathClip, transform.position);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}
