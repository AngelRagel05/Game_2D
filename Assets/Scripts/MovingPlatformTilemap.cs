using UnityEngine;

[DisallowMultipleComponent]
public class MovingPlatformTilemap : MonoBehaviour
{
    public enum EjeMovimiento
    {
        Horizontal,
        Vertical
    }

    [Header("Movimiento")]
    public EjeMovimiento eje = EjeMovimiento.Horizontal;
    public float distancia = 3f;
    public float velocidad = 1.5f;
    public bool iniciarHaciaPositivo = true;

    [Header("Opciones")]
    public bool usarRigidbodyKinematic = true;

    private Rigidbody2D rb;
    private Vector3 posicionInicial;
    private float direccion = 1f;

    private void Awake()
    {
        posicionInicial = transform.position;
        direccion = iniciarHaciaPositivo ? 1f : -1f;
        rb = GetComponent<Rigidbody2D>();

        if (usarRigidbodyKinematic && rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        if (rb != null && usarRigidbodyKinematic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    private void FixedUpdate()
    {
        if (distancia <= 0f || velocidad <= 0f) return;

        Vector3 offset = ObtenerOffsetActual();
        Vector3 objetivo = posicionInicial + offset;

        if (rb != null && usarRigidbodyKinematic)
        {
            rb.MovePosition(objetivo);
        }
        else
        {
            transform.position = objetivo;
        }
    }

    private Vector3 ObtenerOffsetActual()
    {
        float recorrido = Mathf.PingPong(Time.time * velocidad, distancia * 2f) - distancia;
        float valor = recorrido * direccion;

        if (eje == EjeMovimiento.Horizontal)
        {
            return new Vector3(valor, 0f, 0f);
        }

        return new Vector3(0f, valor, 0f);
    }
}
