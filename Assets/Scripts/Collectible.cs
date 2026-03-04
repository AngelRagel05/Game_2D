using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [Header("Visual")]
    public bool rotar = true;
    public float velocidadRotacion = 120f;

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
        if (other.GetComponent<VagabundoController>() == null) return;

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.RegistrarColeccionable();
        }

        Destroy(gameObject);
    }
}
