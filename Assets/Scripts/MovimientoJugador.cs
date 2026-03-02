using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;
    private Rigidbody2D rb;
    private bool enEscalera;
    private float entradaVertical;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        // Movimiento horizontal
        float movH = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(movH * velocidad, rb.linearVelocity.y);

        // Salto
        if (Input.GetButtonDown("Jump") && Mathf.Abs(rb.linearVelocity.y) < 0.01f) {
            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
        }

        // Detectar entrada para trepar
        entradaVertical = Input.GetAxis("Vertical");
    }

    void FixedUpdate() {
        if (enEscalera) {
            rb.gravityScale = 0; // Quitamos gravedad para trepar
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, entradaVertical * velocidad);
        } else {
            rb.gravityScale = 3; // Gravedad normal
        }
    }

    // Detectar si tocamos la escalera (Trigger)
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Escalera")) enEscalera = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Escalera")) enEscalera = false;
    }
}