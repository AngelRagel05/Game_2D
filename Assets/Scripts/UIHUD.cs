using System.Collections.Generic;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    private static readonly Color ColorPanel = new Color(0.08f, 0.1f, 0.14f, 0.92f);
    private static readonly Color ColorBorde = new Color(0.18f, 0.72f, 0.95f, 1f);
    private static readonly Color ColorAcento = new Color(0.95f, 0.72f, 0.18f, 1f);
    private static readonly Color ColorFila = new Color(1f, 1f, 1f, 0.08f);

    [Header("HUD")]
    public bool mostrarHUD = true;
    public Vector2 posicion = new Vector2(12f, 12f);

    [Header("Pausa")]
    public bool permitirPausaConEsc = false;
    public bool mostrarBotonSonido = true;
    public Texture2D fondoPausa;

    private GameManager gm;
    private bool mostrarPanelSonido;

    public void TogglePanelSonido()
    {
        mostrarPanelSonido = !mostrarPanelSonido;
    }

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

        if (permitirPausaConEsc && !gm.PermitirPausa && Input.GetKeyDown(KeyCode.Escape))
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

        if (mostrarBotonSonido)
        {
            DibujarControlSonido();
        }
    }

    private void DibujarHUDBase()
    {
        GUIStyle hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        Rect hudRect = new Rect(posicion.x - 8f, posicion.y - 8f, 285f, 108f);
        DibujarPanel(hudRect, ColorPanel, ColorBorde, 2f);
        DibujarBarraAcento(new Rect(hudRect.x, hudRect.y, hudRect.width, 4f), ColorAcento);

        DibujarLabelConSombra(new Rect(posicion.x, posicion.y, 420f, 32f), $"Tiempo: {GameManager.FormatearTiempo(gm.TiempoTranscurrido)}", hudStyle);
        DibujarLabelConSombra(new Rect(posicion.x, posicion.y + 30f, 420f, 32f), $"Llaves: {gm.ColeccionablesRecogidos}/{gm.TotalColeccionablesNivel}", hudStyle);
        DibujarLabelConSombra(new Rect(posicion.x, posicion.y + 60f, 420f, 32f), "Nivel: 1", hudStyle);
    }

    private void DibujarMenuPausa()
    {
        if (fondoPausa != null)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fondoPausa, ScaleMode.ScaleAndCrop);
        }
        DibujarOverlay(0.65f);

        float w = Screen.width;
        float h = Screen.height;
        float bw = 220f;
        float bh = 42f;
        float x = (w * 0.35f) - (bw * 0.5f);
        float y = h * 0.33f;
        Rect panel = new Rect(x - 24f, y - 110f, bw + 48f, 232f);
        DibujarPanel(panel, ColorPanel, ColorBorde, 2f);
        DibujarBarraAcento(new Rect(panel.x, panel.y, panel.width, 4f), ColorAcento);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        DibujarLabelConSombra(new Rect(x - 10f, y - 80f, bw + 20f, 50f), "PAUSA", titleStyle);

        if (BotonMenu(new Rect(x, y, bw, bh), "CONTINUAR"))
        {
            gm.Reanudar();
        }
        y += 52f;

        if (BotonMenu(new Rect(x, y, bw, bh), "REINICIAR"))
        {
            gm.ReiniciarNivel();
        }
        y += 52f;

        if (BotonMenu(new Rect(x, y, bw, bh), "SALIR"))
        {
            gm.IrMenu();
        }
        y += 52f;

        if (BotonMenu(new Rect(x, y, bw, bh), mostrarPanelSonido ? "CERRAR SONIDO" : "SONIDO"))
        {
            TogglePanelSonido();
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
        Rect panel = new Rect(x - 36f, y - 108f, bw + 72f, 276f);
        DibujarPanel(panel, ColorPanel, ColorBorde, 2f);
        DibujarBarraAcento(new Rect(panel.x, panel.y, panel.width, 4f), ColorAcento);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUIStyle lineStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        DibujarLabelConSombra(new Rect(x - 40f, y - 78f, bw + 80f, 46f), "NIVEL COMPLETADO", titleStyle);
        DibujarLabelConSombra(new Rect(x - 40f, y - 30f, bw + 80f, 26f), $"Tiempo final: {GameManager.FormatearTiempo(gm.TiempoFinalRun)}", lineStyle);
        DibujarLabelConSombra(new Rect(x - 40f, y - 4f, bw + 80f, 26f), $"Llaves: {gm.ColeccionablesRecogidos}/{gm.TotalColeccionablesNivel}", lineStyle);

        if (BotonMenu(new Rect(x, y + 66f, bw, bh), "REINTENTAR"))
        {
            gm.ReiniciarNivel();
        }

        if (BotonMenu(new Rect(x, y + 120f, bw, bh), "SALIR"))
        {
            gm.IrMenu();
        }

        DibujarTop5(new Rect(w * 0.58f, h * 0.22f, 320f, 320f), "TOP 5 TIEMPOS");
    }

    private void DibujarTop5(Rect rect, string titulo)
    {
        DibujarPanel(rect, ColorPanel, ColorBorde, 2f);
        DibujarBarraAcento(new Rect(rect.x, rect.y, rect.width, 4f), ColorAcento);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        DibujarLabelConSombra(new Rect(rect.x, rect.y + 8f, rect.width, 30f), titulo, titleStyle);

        List<float> top = gm.ObtenerTop5();
        float y = rect.y + 44f;
        GUIStyle rowStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };

        for (int i = 0; i < 5; i++)
        {
            if (i % 2 == 0)
            {
                Color prev = GUI.color;
                GUI.color = ColorFila;
                GUI.DrawTexture(new Rect(rect.x + 10f, y + 2f, rect.width - 20f, 22f), Texture2D.whiteTexture);
                GUI.color = prev;
            }

            string valor = i < top.Count ? GameManager.FormatearTiempo(top[i]) : "--:--.--";
            GUI.Label(new Rect(rect.x + 16f, y, rect.width - 24f, 24f), $"{i + 1}. {valor}", rowStyle);
            y += 24f;
        }
    }

    private static void DibujarPanel(Rect rect, Color fondo, Color borde, float grosorBorde)
    {
        Color prev = GUI.color;
        GUI.color = fondo;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = borde;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, grosorBorde), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - grosorBorde, rect.width, grosorBorde), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, grosorBorde, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - grosorBorde, rect.y, grosorBorde, rect.height), Texture2D.whiteTexture);
        GUI.color = prev;
    }

    private static void DibujarBarraAcento(Rect rect, Color color)
    {
        Color prev = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = prev;
    }

    private static void DibujarLabelConSombra(Rect rect, string texto, GUIStyle style)
    {
        Color prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.Label(new Rect(rect.x + 1.5f, rect.y + 1.5f, rect.width, rect.height), texto, style);
        GUI.color = prev;
        GUI.Label(rect, texto, style);
    }

    private static bool BotonMenu(Rect rect, string texto)
    {
        GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };

        Color prevBg = GUI.backgroundColor;
        Color prevContent = GUI.contentColor;
        GUI.backgroundColor = new Color(0.2f, 0.65f, 0.9f, 1f);
        GUI.contentColor = Color.white;
        bool clicked = GUI.Button(rect, texto, btnStyle);
        GUI.backgroundColor = prevBg;
        GUI.contentColor = prevContent;
        return clicked;
    }

    private static void DibujarOverlay(float alpha)
    {
        Color prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = prev;
    }

    private void DibujarControlSonido()
    {
        if (!mostrarPanelSonido) return;

        float w = Screen.width;
        Rect panel = new Rect(w - 470f, 56f, 456f, 336f);
        DibujarPanel(panel, ColorPanel, ColorBorde, 2f);
        DibujarBarraAcento(new Rect(panel.x, panel.y, panel.width, 4f), ColorAcento);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 21,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        DibujarLabelConSombra(new Rect(panel.x, panel.y + 10f, panel.width, 28f), "AJUSTES DE SONIDO", titleStyle);

        float x = panel.x + 14f;
        float y = panel.y + 52f;
        DibujarSliderSonido(labelStyle, x, ref y, "Musica", AudioSettingsStore.Music, AudioSettingsStore.SetMusic);
        DibujarSliderSonido(labelStyle, x, ref y, "Salto", AudioSettingsStore.Jump, AudioSettingsStore.SetJump);
        DibujarSliderSonido(labelStyle, x, ref y, "Correr", AudioSettingsStore.Run, AudioSettingsStore.SetRun);
        DibujarSliderSonido(labelStyle, x, ref y, "Dano", AudioSettingsStore.Damage, AudioSettingsStore.SetDamage);
        DibujarSliderSonido(labelStyle, x, ref y, "Victoria", AudioSettingsStore.Victory, AudioSettingsStore.SetVictory);
        DibujarSliderSonido(labelStyle, x, ref y, "Coleccionable", AudioSettingsStore.Collectible, AudioSettingsStore.SetCollectible);
        DibujarSliderSonido(labelStyle, x, ref y, "Pausa", AudioSettingsStore.Pause, AudioSettingsStore.SetPause);
    }

    private void DibujarSliderSonido(GUIStyle style, float x, ref float y, string nombre, float actual, System.Action<float> setter)
    {
        GUI.Label(new Rect(x, y, 170f, 24f), nombre, style);
        float nuevo = GUI.HorizontalSlider(new Rect(x + 170f, y + 8f, 196f, 16f), actual, 0f, 1f);
        GUI.Label(new Rect(x + 374f, y, 64f, 24f), $"{Mathf.RoundToInt(actual * 100f)}%", style);
        y += 38f;

        if (Mathf.Abs(nuevo - actual) > 0.001f)
        {
            setter(nuevo);
            if (gm != null)
            {
                gm.CargarAjustesAudio();
            }

            BackgroundMusicPlayer bmp = BackgroundMusicPlayer.Instance;
            if (bmp != null)
            {
                bmp.CargarVolumenDesdeAjustes();
            }
        }
    }
}
