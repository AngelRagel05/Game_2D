using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class Tilemap_Coleccionables : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap tilemapColeccionables;
    public TileBase tileLlave;
    public bool usarCualquierTileComoLlave = false;

    [Header("Instanciacion")]
    public GameObject prefabLlave;
    public Sprite spriteLlave;
    public Vector3 escalaLlave = Vector3.one;
    public float radioTrigger = 0.25f;
    public Vector3 offsetMundo = new Vector3(0f, 0f, 0f);
    public bool limpiarTileTrasInstanciar = true;
    public bool sincronizarTotalConGameManager = true;

    private void Reset()
    {
        if (tilemapColeccionables == null)
        {
            tilemapColeccionables = GetComponent<Tilemap>();
        }
    }

    private void Start()
    {
        GenerarLlavesDesdeTilemap();
    }

    [ContextMenu("Generar Llaves Desde Tilemap")]
    public void GenerarLlavesDesdeTilemap()
    {
        if (tilemapColeccionables == null)
        {
            Debug.LogWarning($"{nameof(Tilemap_Coleccionables)}: faltan referencias de Tilemap.", this);
            return;
        }

        if (prefabLlave == null && spriteLlave == null)
        {
            Debug.LogWarning($"{nameof(Tilemap_Coleccionables)}: asigna un prefab o un sprite para crear llaves.", this);
            return;
        }

        int generadas = 0;
        BoundsInt bounds = tilemapColeccionables.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            TileBase tile = tilemapColeccionables.GetTile(cell);
            if (tile == null) continue;

            bool esLlave = usarCualquierTileComoLlave || tile == tileLlave;
            if (!esLlave) continue;

            Vector3 spawnPos = tilemapColeccionables.GetCellCenterWorld(cell) + offsetMundo;
            if (prefabLlave != null)
            {
                Instantiate(prefabLlave, spawnPos, Quaternion.identity, transform);
            }
            else
            {
                CrearLlaveDesdeSprite(spawnPos);
            }
            generadas++;

            if (limpiarTileTrasInstanciar)
            {
                tilemapColeccionables.SetTile(cell, null);
            }
        }

        if (limpiarTileTrasInstanciar)
        {
            tilemapColeccionables.CompressBounds();
        }

        if (sincronizarTotalConGameManager)
        {
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                gm.ConfigurarTotalColeccionablesNivel(generadas);
            }
        }
    }

    private void CrearLlaveDesdeSprite(Vector3 posicion)
    {
        GameObject go = new GameObject("Llave");
        go.transform.SetParent(transform);
        go.transform.position = posicion;
        go.transform.localScale = escalaLlave;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spriteLlave;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = Mathf.Max(0.01f, radioTrigger);

        go.AddComponent<Collectible>();
    }
}
