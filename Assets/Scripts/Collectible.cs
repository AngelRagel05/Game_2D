using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [Header("Visual")]
    public bool rotar = true;
    public float velocidadRotacion = 120f;
    private bool recogido;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (rotar)
        {
            transform.Rotate(0f, 0f, velocidadRotacion * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IntentarRecoger(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        IntentarRecoger(other);
    }

    private void IntentarRecoger(Collider2D other)
    {
        if (recogido) return;
        if (!EsJugador(other)) return;
        recogido = true;

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.RegistrarColeccionable();
        }

        Destroy(gameObject);
    }

    private static bool EsJugador(Collider2D other)
    {
        if (other == null) return false;

        if (other.GetComponent<VagabundoController>() != null) return true;
        if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<VagabundoController>() != null) return true;
        if (other.GetComponentInParent<VagabundoController>() != null) return true;

        return false;
    }
}
