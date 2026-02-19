using UnityEngine;

/// <summary>
/// Attach this to spike/hazard GameObjects.
/// The player dies instantly on contact.
/// </summary>
public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
        }
    }
}
