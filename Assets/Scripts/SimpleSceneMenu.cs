using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneMenu : MonoBehaviour
{
    [Header("Escenas")]
    public string escenaJugar = "SampleScene";
    public string escenaMenu = "MainMenu";

    [Header("Textos")]
    public string titulo = "ZEROHOUR";
    public string subtitulo = "Escapa antes de que llegue a 00:00";
    public bool mostrarBotonJugar = true;
    public bool mostrarBotonMenu = false;
    public bool mostrarBotonReintentar = false;
    public bool mostrarBotonSalir = true;

    [Header("Ranking")]
    public bool mostrarRankingEnGameOver = true;
    public string escenaRanking = "Victory";

    private const string HighscorePrefix = "ZH_BEST_";
    private const int MaxHighscores = 5;

    private void OnGUI()
    {
        float w = Screen.width;
        float h = Screen.height;
        bool esEscenaRanking = SceneManager.GetActiveScene().name == escenaRanking;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };

        GUI.Label(new Rect(0f, h * 0.15f, w, 50f), titulo, titleStyle);
        GUI.Label(new Rect(0f, h * 0.23f, w, 30f), subtitulo, subStyle);

        if (!esEscenaRanking)
        {
            float bw = 220f;
            float bh = 42f;
            float x = (w - bw) * 0.5f;
            float y = h * 0.45f;

            if (mostrarBotonJugar)
            {
                if (GUI.Button(new Rect(x, y, bw, bh), "JUGAR"))
                {
                    if (!string.IsNullOrWhiteSpace(escenaJugar)) SceneManager.LoadScene(escenaJugar);
                }
                y += 52f;
            }

            if (mostrarBotonReintentar)
            {
                if (GUI.Button(new Rect(x, y, bw, bh), "REINTENTAR"))
                {
                    SceneManager.LoadScene(escenaJugar);
                }
                y += 52f;
            }

            if (mostrarBotonMenu)
            {
                if (GUI.Button(new Rect(x, y, bw, bh), "MENU"))
                {
                    if (!string.IsNullOrWhiteSpace(escenaMenu)) SceneManager.LoadScene(escenaMenu);
                }
                y += 52f;
            }

            if (mostrarBotonSalir)
            {
                if (GUI.Button(new Rect(x, y, bw, bh), "SALIR"))
                {
                    Application.Quit();
                }
            }
        }

        if (mostrarRankingEnGameOver && esEscenaRanking)
        {
            DibujarRankingGameOver(w, h);
        }
    }

    private void DibujarRankingGameOver(float w, float h)
    {
        Rect box = new Rect(w * 0.67f, h * 0.26f, 280f, 240f);
        GUI.Box(box, "TOP 5 TIEMPOS");

        float ultimo = GameManager.ObtenerUltimoTiempo();
        string ultimoTexto = ultimo >= 0f ? GameManager.FormatearTiempo(ultimo) : "--:--.--";
        GUI.Label(new Rect(box.x + 12f, box.y + 28f, box.width - 20f, 22f), $"Ultimo: {ultimoTexto}");

        float y = box.y + 58f;
        for (int i = 0; i < MaxHighscores; i++)
        {
            string key = HighscorePrefix + i;
            string valor = PlayerPrefs.HasKey(key) ? GameManager.FormatearTiempo(PlayerPrefs.GetFloat(key)) : "--:--.--";
            GUI.Label(new Rect(box.x + 12f, y, box.width - 20f, 22f), $"{i + 1}. {valor}");
            y += 22f;
        }
    }
}
