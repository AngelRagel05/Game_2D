using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float decceleration = 50f;
    [SerializeField] private float velPower = 0.9f;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canDash = true;
    private bool isDashing;
    private float horizontalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDashing) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Jump logic
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Dash logic
        if (Input.GetKeyDown(KeyCode.X) && canDash)
        {
            StartCoroutine(Dash());
        }

        CheckGroud();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // Calculate target speed and speed difference
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;

        // Change acceleration rate depending on situation
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;

        // Apply acceleration, power and multiplication by movement speed
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Dash direction (if no input, dash forward based on facing direction, simplifying here)
        Vector2 dashDir = horizontalInput != 0 ? new Vector2(horizontalInput, 0) : (transform.localScale.x > 0 ? Vector2.right : Vector2.left);
        rb.velocity = dashDir.normalized * dashForce;

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        // We only reset canDash when grounded in Celeste, but here we use a timer for simpler logic first
        // unless you want the exact Celeste feel where it resets on ground touch.
    }

    private void CheckGroud()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
        
        if (isGrounded && !wasGrounded)
        {
            canDash = true; // Reset dash when touching ground
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }

    public void Die()
    {
        // Respawns at GameManager's specified checkpoint
        GameManager.Instance.RespawnPlayer();
    }
}
