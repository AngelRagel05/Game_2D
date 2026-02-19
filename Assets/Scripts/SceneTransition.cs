using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles fade-in / fade-out transitions between scenes.
/// Add a Canvas with a full-screen black Image as a child of this GameObject.
/// Assign that Image to the 'fadeImage' field.
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Fade in on scene start
        StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            Color c = fadeImage.color;
            c.a = 1f - (t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        Color fc = fadeImage.color;
        fc.a = 0;
        fadeImage.color = fc;
    }

    private IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            Color c = fadeImage.color;
            c.a = t / fadeDuration;
            fadeImage.color = c;
            yield return null;
        }
        Color fc = fadeImage.color;
        fc.a = 1;
        fadeImage.color = fc;
    }
}
