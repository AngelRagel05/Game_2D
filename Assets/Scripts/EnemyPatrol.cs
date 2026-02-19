using UnityEngine;

/// <summary>
/// Simple left-right patrol enemy.
/// The enemy kills the player on contact.
/// Requires two child GameObjects named "LeftPoint" and "RightPoint" as patrol bounds,
/// OR just set patrolDistance.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 startPos;
    private int direction = 1;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

        // Flip direction when patrol bound is reached
        float traveled = transform.position.x - startPos.x;
        if (traveled >= patrolDistance || traveled <= -patrolDistance)
        {
            direction *= -1;
            // Flip sprite
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
        }
    }
}
