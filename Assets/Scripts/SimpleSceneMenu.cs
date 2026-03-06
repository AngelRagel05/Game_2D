using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneMenu : MonoBehaviour
{
    private static readonly Color OverlayColor = new Color(0.03f, 0.05f, 0.09f, 0.72f);
    private static readonly Color PanelColor = new Color(0.08f, 0.12f, 0.18f, 0.92f);
    private static readonly Color BorderColor = new Color(0.22f, 0.7f, 0.92f, 1f);
    private static readonly Color AccentColor = new Color(0.96f, 0.72f, 0.2f, 1f);

    [Header("Escenas")]
    public string escenaJugar = "SampleScene";
    public string escenaMenu = "MainMenu";

    [Header("Textos")]
    public string titulo = "ZEROHOUR";
    public string subtitulo = "ESCAPA LO ANTES POSIBLE";
    public bool mostrarBotonJugar = true;
    public bool mostrarBotonMenu = false;
    public bool mostrarBotonReintentar = false;
    public bool mostrarBotonSalir = true;

    [Header("Ranking")]
    public bool mostrarRankingEnGameOver = true;
    public string escenaRanking = "Victory";
    public bool mostrarBotonSonido = true;

    [Header("Audio Bootstrap")]
    public bool crearMusicPlayerSiFalta = true;
    public AudioClip[] playlistGameplay;
    public AudioClip clipMenuPausa;
    public AudioClip clipVictoria;
    public float volumenMusicaDefault = 0.35f;
    public float startAtSeconds = 5f;

    private const string HighscorePrefix = "ZH_BEST_";
    private const int MaxHighscores = 5;
    private bool mostrarPanelSonido;
    private Canvas canvasExterno;

    public void TogglePanelSonido()
    {
        mostrarPanelSonido = !mostrarPanelSonido;
    }

    private void Start()
    {
        canvasExterno = FindFirstObjectByType<Canvas>();
        AsegurarMusicPlayer();
    }

    private void OnGUI()
    {
        float w = Screen.width;
        float h = Screen.height;
        bool esEscenaRanking = SceneManager.GetActiveScene().name == escenaRanking;
        bool usarUIExterna = canvasExterno != null;

        if (!usarUIExterna)
        {
            DibujarOverlay(OverlayColor.a);
        }

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 1f, 1f, 0.9f) }
        };

        if (!usarUIExterna)
        {
            Rect heroPanel = new Rect((w * 0.5f) - 300f, h * 0.10f, 600f, 130f);
            DibujarPanel(heroPanel, PanelColor, BorderColor, 2f);
            DibujarBarraAcento(new Rect(heroPanel.x, heroPanel.y, heroPanel.width, 4f), AccentColor);
            DibujarLabelConSombra(new Rect(0f, h * 0.15f, w, 50f), titulo, titleStyle);
            DibujarLabelConSombra(new Rect(0f, h * 0.23f, w, 30f), subtitulo, subStyle);
        }

        if (!esEscenaRanking && !usarUIExterna)
        {
            float bw = 220f;
            float bh = 42f;
            float x = (w - bw) * 0.5f;
            float y = h * 0.45f;
            int botones = (mostrarBotonJugar ? 1 : 0) + (mostrarBotonReintentar ? 1 : 0) + (mostrarBotonMenu ? 1 : 0) + (mostrarBotonSalir ? 1 : 0);
            Rect menuPanel = new Rect(x - 24f, y - 26f, bw + 48f, Mathf.Max(84f, botones * 52f + 20f));
            DibujarPanel(menuPanel, PanelColor, BorderColor, 2f);
            DibujarBarraAcento(new Rect(menuPanel.x, menuPanel.y, menuPanel.width, 4f), AccentColor);

            if (mostrarBotonJugar)
            {
                if (BotonMenu(new Rect(x, y, bw, bh), "JUGAR"))
                {
                    if (!string.IsNullOrWhiteSpace(escenaJugar)) SceneManager.LoadScene(escenaJugar);
                }
                y += 52f;
            }

            if (mostrarBotonReintentar)
            {
                if (BotonMenu(new Rect(x, y, bw, bh), "REINTENTAR"))
                {
                    SceneManager.LoadScene(escenaJugar);
                }
                y += 52f;
            }

            if (mostrarBotonMenu)
            {
                if (BotonMenu(new Rect(x, y, bw, bh), "MENU"))
                {
                    if (!string.IsNullOrWhiteSpace(escenaMenu)) SceneManager.LoadScene(escenaMenu);
                }
                y += 52f;
            }

            if (mostrarBotonSalir)
            {
                if (BotonMenu(new Rect(x, y, bw, bh), "SALIR"))
                {
                    Application.Quit();
                }
            }
        }

        if (mostrarRankingEnGameOver && esEscenaRanking)
        {
            DibujarRankingGameOver(w, h);
        }

        if (mostrarBotonSonido)
        {
            DibujarControlSonido();
        }
    }

    private void DibujarRankingGameOver(float w, float h)
    {
        Rect box = new Rect(w * 0.67f, h * 0.26f, 280f, 240f);
        DibujarPanel(box, PanelColor, BorderColor, 2f);
        DibujarBarraAcento(new Rect(box.x, box.y, box.width, 4f), AccentColor);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        DibujarLabelConSombra(new Rect(box.x, box.y + 8f, box.width, 26f), "TOP 5 TIEMPOS", titleStyle);

        float ultimo = GameManager.ObtenerUltimoTiempo();
        string ultimoTexto = ultimo >= 0f ? GameManager.FormatearTiempo(ultimo) : "--:--.--";
        GUIStyle rowStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(box.x + 12f, box.y + 38f, box.width - 20f, 22f), $"Ultimo: {ultimoTexto}", rowStyle);

        float y = box.y + 68f;
        for (int i = 0; i < MaxHighscores; i++)
        {
            string key = HighscorePrefix + i;
            string valor = PlayerPrefs.HasKey(key) ? GameManager.FormatearTiempo(PlayerPrefs.GetFloat(key)) : "--:--.--";
            GUI.Label(new Rect(box.x + 12f, y, box.width - 20f, 22f), $"{i + 1}. {valor}", rowStyle);
            y += 22f;
        }
    }

    private static void DibujarOverlay(float alpha)
    {
        Color prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = prev;
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

    private void DibujarControlSonido()
    {
        if (!mostrarPanelSonido) return;

        float w = Screen.width;
        Rect panel = new Rect(w - 470f, 56f, 456f, 336f);
        DibujarPanel(panel, PanelColor, BorderColor, 2f);
        DibujarBarraAcento(new Rect(panel.x, panel.y, panel.width, 4f), AccentColor);

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

    private static void DibujarSliderSonido(GUIStyle style, float x, ref float y, string nombre, float actual, System.Action<float> setter)
    {
        GUI.Label(new Rect(x, y, 170f, 24f), nombre, style);
        float nuevo = GUI.HorizontalSlider(new Rect(x + 170f, y + 8f, 196f, 16f), actual, 0f, 1f);
        GUI.Label(new Rect(x + 374f, y, 64f, 24f), $"{Mathf.RoundToInt(actual * 100f)}%", style);
        y += 38f;

        if (Mathf.Abs(nuevo - actual) > 0.001f)
        {
            setter(nuevo);
            BackgroundMusicPlayer bmp = BackgroundMusicPlayer.Instance;
            if (bmp != null)
            {
                bmp.CargarVolumenDesdeAjustes();
            }
        }
    }

    private void AsegurarMusicPlayer()
    {
        if (!crearMusicPlayerSiFalta) return;
        if (BackgroundMusicPlayer.Instance != null) return;

        GameObject go = new GameObject("MusicPlayer");
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f;

        BackgroundMusicPlayer bmp = go.AddComponent<BackgroundMusicPlayer>();
        bmp.dontDestroyOnLoad = true;
        bmp.playlist = playlistGameplay;
        bmp.pauseMenuClip = clipMenuPausa;
        bmp.victoryClip = clipVictoria;
        bmp.mainMenuSceneName = escenaMenu;
        bmp.victorySceneName = escenaRanking;
        bmp.volume = Mathf.Clamp01(volumenMusicaDefault);
        bmp.startAtSeconds = Mathf.Max(0f, startAtSeconds);
        if (playlistGameplay != null && playlistGameplay.Length > 0)
        {
            bmp.musicClip = playlistGameplay[0];
        }
    }
}
