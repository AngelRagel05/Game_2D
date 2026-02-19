using UnityEngine;

/// <summary>
/// Attach this to strawberry/collectible GameObjects.
/// The player collects it on contact, earning points.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private AudioClip collectSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            // Play sound (detached so it survives the GameObject being destroyed)
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            GameManager.Instance.AddScore(scoreValue);
            Destroy(gameObject);
        }
    }
}
