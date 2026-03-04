using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Partida")]
    public float tiempoInicial = 90f;
    public bool muerteInstantanea = true;
    public float delayReinicioMuerte = 0.05f;

    [Header("Coleccionables")]
    public int totalColeccionablesNivel = 3;

    [Header("Final")]
    public bool mostrarPantallaFinalEnNivel = true;
    public string escenaVictoria = "Victory";

    [Header("Pausa")]
    public bool permitirPausa = true;
    public string escenaMenu = "MainMenu";

    [Header("HUD Fallback")]
    public bool mostrarHUDFallback = true;
    public Vector2 hudPosicion = new Vector2(12f, 12f);

    [Header("Feedback Daño")]
    public float flashRojoDuracion = 0.35f;
    [Range(0f, 1f)] public float flashRojoAlphaMax = 0.35f;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip saltoSfx;
    public AudioClip danoSfx;
    public AudioClip checkpointSfx;
    public AudioClip victoriaSfx;
    public AudioClip coleccionableSfx;

    private const string HighscorePrefix = "ZH_BEST_";
    private const string LastRunKey = "ZH_LAST_RUN";
    private const int MaxHighscores = 5;

    private VagabundoController player;
    private bool tieneHUDExterno;
    private bool nivelTerminado;
    private bool reiniciando;
    private bool pausado;
    private float tiempoRestante;
    private float flashRojoTimer;
    private int coleccionablesRecogidos;
    private float tiempoFinalRun;

    public float TiempoRestante => tiempoRestante;
    public bool NivelTerminado => nivelTerminado;
    public bool Pausado => pausado;
    public int ColeccionablesRecogidos => coleccionablesRecogidos;
    public int TotalColeccionablesNivel => Mathf.Max(0, totalColeccionablesNivel);
    public bool CompletadoAl100 => TotalColeccionablesNivel > 0 && coleccionablesRecogidos >= TotalColeccionablesNivel;
    public float TiempoFinalRun => tiempoFinalRun;
    public float TiempoTranscurrido => Mathf.Clamp(tiempoInicial - tiempoRestante, 0f, tiempoInicial);
    public bool MuestraPantallaFinal => nivelTerminado && mostrarPantallaFinalEnNivel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        player = FindFirstObjectByType<VagabundoController>();
        tieneHUDExterno = FindFirstObjectByType<UIHUD>() != null;
        tiempoRestante = tiempoInicial;
        coleccionablesRecogidos = 0;
    }

    private void Update()
    {
        if (permitirPausa && !nivelTerminado && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausa();
        }

        if (nivelTerminado || pausado) return;

        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            StartCoroutine(ReiniciarNivelConDelay(0f));
            return;
        }

        if (flashRojoTimer > 0f)
        {
            flashRojoTimer -= Time.deltaTime;
        }
    }

    public void TogglePausa()
    {
        if (!permitirPausa || nivelTerminado) return;
        SetPausa(!pausado);
    }

    public void SetPausa(bool value)
    {
        if (value && (!permitirPausa || nivelTerminado)) return;
        pausado = value;
        Time.timeScale = pausado ? 0f : 1f;
    }

    public void Reanudar()
    {
        SetPausa(false);
    }

    public void RegistrarCheckpoint(Vector3 spawn)
    {
        if (player != null)
        {
            player.SetSpawnPoint(spawn);
            PlaySfx(checkpointSfx);
        }
    }

    public void RegistrarColeccionable()
    {
        if (nivelTerminado) return;
        coleccionablesRecogidos = Mathf.Min(coleccionablesRecogidos + 1, TotalColeccionablesNivel);
        PlaySfx(coleccionableSfx);
    }

    public void DanoJugador(int dano = 1)
    {
        Vector2 fallbackFuente = player != null ? (Vector2)player.transform.position : Vector2.zero;
        DanoJugador(dano, fallbackFuente);
    }

    public void DanoJugador(int dano, Vector2 fuenteDanio)
    {
        if (nivelTerminado || reiniciando) return;

        flashRojoTimer = flashRojoDuracion;
        PlaySfx(danoSfx);
        if (player != null)
        {
            player.AplicarEmpujeDanio(fuenteDanio);
        }

        if (muerteInstantanea)
        {
            StartCoroutine(ReiniciarNivelConDelay(delayReinicioMuerte));
        }
    }

    public void DanoJugadorPorLava(int dano, Vector2 fuenteDanio)
    {
        DanoJugador(dano, fuenteDanio);
    }

    public void GanarPartida()
    {
        if (nivelTerminado) return;

        SetPausa(false);
        nivelTerminado = true;
        PlaySfx(victoriaSfx);

        tiempoFinalRun = TiempoTranscurrido;
        GuardarTiempoTop5(tiempoFinalRun);
        PlayerPrefs.SetFloat(LastRunKey, tiempoFinalRun);
        PlayerPrefs.Save();

        if (!string.IsNullOrWhiteSpace(escenaVictoria))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(escenaVictoria);
        }
    }

    public void PlayJumpSfx()
    {
        PlaySfx(saltoSfx);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }

    public void IrMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrWhiteSpace(escenaMenu))
        {
            SceneManager.LoadScene(escenaMenu);
        }
    }

    public List<float> ObtenerTop5()
    {
        List<float> tiempos = new List<float>(MaxHighscores);
        for (int i = 0; i < MaxHighscores; i++)
        {
            string key = HighscorePrefix + i;
            if (PlayerPrefs.HasKey(key))
            {
                tiempos.Add(PlayerPrefs.GetFloat(key));
            }
        }
        return tiempos;
    }

    public static float ObtenerUltimoTiempo()
    {
        return PlayerPrefs.GetFloat(LastRunKey, -1f);
    }

    public static string FormatearTiempo(float tiempo)
    {
        float t = Mathf.Max(0f, tiempo);
        int minutos = Mathf.FloorToInt(t / 60f);
        int segundos = Mathf.FloorToInt(t % 60f);
        int centesimas = Mathf.FloorToInt((t - Mathf.Floor(t)) * 100f);
        return $"{minutos:00}:{segundos:00}.{centesimas:00}";
    }

    private IEnumerator ReiniciarNivelConDelay(float delay)
    {
        if (reiniciando) yield break;
        reiniciando = true;
        if (delay > 0f) yield return new WaitForSeconds(delay);
        ReiniciarNivel();
    }

    private void GuardarTiempoTop5(float tiempo)
    {
        List<float> tiempos = ObtenerTop5();
        tiempos.Add(tiempo);
        tiempos.Sort();

        if (tiempos.Count > MaxHighscores)
        {
            tiempos.RemoveRange(MaxHighscores, tiempos.Count - MaxHighscores);
        }

        for (int i = 0; i < tiempos.Count; i++)
        {
            PlayerPrefs.SetFloat(HighscorePrefix + i, tiempos[i]);
        }

        for (int i = tiempos.Count; i < MaxHighscores; i++)
        {
            PlayerPrefs.DeleteKey(HighscorePrefix + i);
        }

        PlayerPrefs.Save();
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private void OnGUI()
    {
        if (mostrarHUDFallback && !tieneHUDExterno)
        {
            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y, 360f, 24f), $"Tiempo: {FormatearTiempo(TiempoTranscurrido)}");
            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y + 22f, 360f, 24f), $"Coleccionables: {coleccionablesRecogidos}/{TotalColeccionablesNivel}");
            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y + 44f, 360f, 24f), pausado ? "PAUSA" : "");
            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y + 66f, 360f, 24f), nivelTerminado ? "NIVEL TERMINADO" : "");
        }

        if (flashRojoTimer > 0f)
        {
            float pct = Mathf.Clamp01(flashRojoTimer / flashRojoDuracion);
            float alpha = flashRojoAlphaMax * pct;
            Color prev = GUI.color;
            GUI.color = new Color(1f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;
        }
    }
}
