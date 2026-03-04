using System.Collections.Generic;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    [Header("HUD")]
    public bool mostrarHUD = true;
    public Vector2 posicion = new Vector2(12f, 12f);

    [Header("Pausa")]
    public bool permitirPausaConEsc = false;

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
    }

    private void Update()
    {
        if (gm == null)
        {
            gm = GameManager.Instance;
            return;
        }

        if (permitirPausaConEsc && Input.GetKeyDown(KeyCode.Escape))
        {
            gm.TogglePausa();
        }
    }

    private void OnGUI()
    {
        if (gm == null) return;

        if (mostrarHUD)
        {
            DibujarHUDBase();
        }

        if (gm.Pausado)
        {
            DibujarMenuPausa();
        }

        if (gm.MuestraPantallaFinal)
        {
            DibujarPantallaFinal();
        }
    }

    private void DibujarHUDBase()
    {
        GUI.Label(new Rect(posicion.x, posicion.y, 340f, 24f), $"Tiempo: {GameManager.FormatearTiempo(gm.TiempoTranscurrido)}");
        GUI.Label(new Rect(posicion.x, posicion.y + 22f, 340f, 24f), $"Coleccionables: {gm.ColeccionablesRecogidos}/{gm.TotalColeccionablesNivel}");
        GUI.Label(new Rect(posicion.x, posicion.y + 44f, 340f, 24f), "Nivel: 1");
    }

    private void DibujarMenuPausa()
    {
        DibujarOverlay(0.65f);

        float w = Screen.width;
        float h = Screen.height;
        float bw = 220f;
        float bh = 42f;
        float x = (w * 0.35f) - (bw * 0.5f);
        float y = h * 0.33f;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Label(new Rect(x - 10f, y - 80f, bw + 20f, 50f), "PAUSA", titleStyle);

        if (GUI.Button(new Rect(x, y, bw, bh), "CONTINUAR"))
        {
            gm.Reanudar();
        }
        y += 52f;

        if (GUI.Button(new Rect(x, y, bw, bh), "REINICIAR"))
        {
            gm.ReiniciarNivel();
        }
        y += 52f;

        if (GUI.Button(new Rect(x, y, bw, bh), "SALIR"))
        {
            gm.IrMenu();
        }

        DibujarTop5(new Rect(w * 0.58f, h * 0.28f, 320f, 260f), "TOP 5 TIEMPOS");
    }

    private void DibujarPantallaFinal()
    {
        DibujarOverlay(0.72f);

        float w = Screen.width;
        float h = Screen.height;
        float bw = 240f;
        float bh = 44f;
        float x = (w * 0.35f) - (bw * 0.5f);
        float y = h * 0.30f;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUIStyle lineStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };

        GUI.Label(new Rect(x - 40f, y - 78f, bw + 80f, 46f), "NIVEL COMPLETADO", titleStyle);
        GUI.Label(new Rect(x - 40f, y - 30f, bw + 80f, 26f), $"Tiempo final: {GameManager.FormatearTiempo(gm.TiempoFinalRun)}", lineStyle);
        GUI.Label(new Rect(x - 40f, y - 4f, bw + 80f, 26f), gm.CompletadoAl100 ? "Completado al 100%" : "No recogiste todos los coleccionables", lineStyle);

        if (GUI.Button(new Rect(x, y + 42f, bw, bh), "REINTENTAR"))
        {
            gm.ReiniciarNivel();
        }

        if (GUI.Button(new Rect(x, y + 96f, bw, bh), "SALIR"))
        {
            gm.IrMenu();
        }

        DibujarTop5(new Rect(w * 0.58f, h * 0.22f, 320f, 320f), "TOP 5 TIEMPOS");
    }

    private void DibujarTop5(Rect rect, string titulo)
    {
        GUI.Box(rect, titulo);

        List<float> top = gm.ObtenerTop5();
        float y = rect.y + 34f;
        for (int i = 0; i < 5; i++)
        {
            string valor = i < top.Count ? GameManager.FormatearTiempo(top[i]) : "--:--.--";
            GUI.Label(new Rect(rect.x + 16f, y, rect.width - 24f, 24f), $"{i + 1}. {valor}");
            y += 24f;
        }
    }

    private static void DibujarOverlay(float alpha)
    {
        Color prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = prev;
    }
}
