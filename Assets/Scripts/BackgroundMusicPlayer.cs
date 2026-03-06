using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicPlayer : MonoBehaviour
{
    public static BackgroundMusicPlayer Instance { get; private set; }

    [Header("Music")]
    public AudioClip musicClip;
    public AudioClip[] playlist;
    public AudioClip pauseMenuClip;
    public AudioClip victoryClip;
    public string mainMenuSceneName = "MainMenu";
    public string victorySceneName = "Victory";
    [Range(0f, 1f)] public float volume = 0.35f;
    [Min(0f)] public float startAtSeconds = 5f;
    public bool dontDestroyOnLoad = true;

    private AudioSource source;
    private bool cambiarCancionEnSiguienteRun;
    private float pausedAtTime;
    private bool hasPausedTime;
    private float pauseMenuResumeTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        source = GetComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = Mathf.Clamp01(volume);

        if (musicClip != null)
        {
            source.clip = musicClip;
        }

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        CargarVolumenDesdeAjustes();
        AplicarMusicaParaEscena(SceneManager.GetActiveScene().name);
    }

    public void SetPaused(bool paused)
    {
        if (source == null || source.clip == null) return;

        if (paused)
        {
            if (source.isPlaying)
            {
                pausedAtTime = source.time;
                hasPausedTime = true;
                source.Pause();
            }
            return;
        }

        if (hasPausedTime)
        {
            float maxTime = Mathf.Max(0f, source.clip.length - 0.01f);
            source.time = Mathf.Clamp(pausedAtTime, 0f, maxTime);
            hasPausedTime = false;
        }

        if (!source.isPlaying)
        {
            source.volume = ObtenerVolumenParaClip(source.clip);
            source.Play();
        }
    }

    public void NotifyVictory()
    {
        cambiarCancionEnSiguienteRun = true;
    }

    public void NotifyRunStart()
    {
        if (source == null) return;

        string currentScene = SceneManager.GetActiveScene().name;
        if (EsEscenaMenuPrincipal(currentScene) || EsEscenaVictoria(currentScene))
        {
            return;
        }

        if (source.clip == null)
        {
            source.clip = ElegirClipAleatorio(-1);
        }

        if (source.clip == null) return;

        if (cambiarCancionEnSiguienteRun)
        {
            int currentIndex = ObtenerIndiceClip(source.clip);
            AudioClip nuevo = ElegirClipAleatorio(currentIndex);
            if (nuevo != null)
            {
                source.clip = nuevo;
                source.time = Mathf.Clamp(startAtSeconds, 0f, Mathf.Max(0f, source.clip.length - 0.01f));
                hasPausedTime = false;
                source.volume = ObtenerVolumenParaClip(source.clip);
                source.Play();
            }

            cambiarCancionEnSiguienteRun = false;
            return;
        }

        if (!source.isPlaying)
        {
            source.volume = ObtenerVolumenParaClip(source.clip);
            source.Play();
        }
    }

    private AudioClip ElegirClipAleatorio(int excluirIndice)
    {
        if (playlist != null && playlist.Length > 0)
        {
            int validCount = 0;
            for (int i = 0; i < playlist.Length; i++)
            {
                if (playlist[i] != null) validCount++;
            }

            if (validCount == 0) return musicClip;
            if (validCount == 1)
            {
                for (int i = 0; i < playlist.Length; i++)
                {
                    if (playlist[i] != null) return playlist[i];
                }
            }

            for (int intentos = 0; intentos < 10; intentos++)
            {
                int idx = Random.Range(0, playlist.Length);
                if (idx == excluirIndice) continue;
                if (playlist[idx] == null) continue;
                return playlist[idx];
            }

            for (int i = 0; i < playlist.Length; i++)
            {
                if (i == excluirIndice) continue;
                if (playlist[i] != null) return playlist[i];
            }
        }

        return musicClip;
    }

    private int ObtenerIndiceClip(AudioClip clip)
    {
        if (clip == null || playlist == null) return -1;
        for (int i = 0; i < playlist.Length; i++)
        {
            if (playlist[i] == clip) return i;
        }
        return -1;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CargarVolumenDesdeAjustes();
        AplicarMusicaParaEscena(scene.name);
    }

    private void AplicarMusicaParaEscena(string sceneName)
    {
        if (source == null) return;
        bool esMenu = EsEscenaMenuPrincipal(sceneName);
        bool esVictoria = EsEscenaVictoria(sceneName);

        if (!esMenu && pauseMenuClip != null && source.clip == pauseMenuClip)
        {
            // Keep pause track position so it can resume on next ESC/menu entry.
            pauseMenuResumeTime = source.time;
        }

        if (esMenu)
        {
            ReproducirClipEscena(pauseMenuClip, pauseMenuResumeTime);
            return;
        }

        if (esVictoria)
        {
            ReproducirClipEscena(victoryClip, 0f);
            return;
        }

        bool clipActualEsGameplay = source.clip != null && EsClipDeGameplay(source.clip);
        if (!clipActualEsGameplay)
        {
            if (playlist != null && playlist.Length > 0)
            {
                source.clip = ElegirClipAleatorio(-1);
            }
            else
            {
                source.clip = musicClip;
            }
        }

        if (playlist != null && playlist.Length > 0 && source.clip == null)
        {
            source.clip = ElegirClipAleatorio(-1);
        }
        else if (source.clip == null)
        {
            source.clip = musicClip;
        }

        if (source.clip == null)
        {
            Debug.LogWarning($"{nameof(BackgroundMusicPlayer)}: falta asignar musicClip o playlist.", this);
            return;
        }

        if (!source.isPlaying)
        {
            float t = Mathf.Clamp(startAtSeconds, 0f, Mathf.Max(0f, source.clip.length - 0.01f));
            source.time = t;
            source.volume = ObtenerVolumenParaClip(source.clip);
            source.Play();
        }
        else if (!clipActualEsGameplay)
        {
            // We switched from pause/victory clip to gameplay; ensure playback starts.
            float t = Mathf.Clamp(startAtSeconds, 0f, Mathf.Max(0f, source.clip.length - 0.01f));
            source.time = t;
            source.volume = ObtenerVolumenParaClip(source.clip);
            source.Play();
        }
    }

    private void ReproducirClipEscena(AudioClip clip, float startTime)
    {
        if (clip == null) return;
        if (source.clip != clip)
        {
            source.Stop();
            source.clip = clip;
        }

        float t = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, source.clip.length - 0.01f));
        source.time = t;
        source.loop = true;
        source.volume = ObtenerVolumenParaClip(source.clip);
        source.Play();
        hasPausedTime = false;
    }

    private bool EsEscenaMenuPrincipal(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(mainMenuSceneName) && sceneName == mainMenuSceneName;
    }

    private bool EsEscenaVictoria(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(victorySceneName) && sceneName == victorySceneName;
    }

    private bool EsClipDeGameplay(AudioClip clip)
    {
        if (clip == null) return false;
        if (pauseMenuClip != null && clip == pauseMenuClip) return false;
        if (victoryClip != null && clip == victoryClip) return false;
        return true;
    }

    public void CargarVolumenDesdeAjustes()
    {
        volume = AudioSettingsStore.Music;
        if (source != null)
        {
            source.volume = ObtenerVolumenParaClip(source.clip);
        }
    }

    private float ObtenerVolumenParaClip(AudioClip clip)
    {
        if (clip == null)
        {
            return Mathf.Clamp01(AudioSettingsStore.Music);
        }

        if (pauseMenuClip != null && clip == pauseMenuClip)
        {
            return Mathf.Clamp01(AudioSettingsStore.Pause);
        }

        if (victoryClip != null && clip == victoryClip)
        {
            return Mathf.Clamp01(AudioSettingsStore.Victory);
        }

        return Mathf.Clamp01(AudioSettingsStore.Music);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
