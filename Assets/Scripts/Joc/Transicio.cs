using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Canvi d'escena amb fosa a negre (imatge i so).
/// Ús:  Transicio.CarregarEscena("Game");
/// Es crea sola la primera vegada i es manté entre escenes (DontDestroyOnLoad).
/// </summary>
public class Transicio : MonoBehaviour
{
    public static float duradaFosa = 0.5f;

    private static Transicio instancia;
    private CanvasGroup grup;
    private bool ocupada;

    /// <summary>Fon a negre, carrega l'escena i torna a aparèixer.</summary>
    public static void CarregarEscena(string escena)
    {
        if (!Application.CanStreamedLevelBeLoaded(escena))
        {
            Debug.LogError("[Transicio] No es pot carregar l'escena '" + escena + "'. Està a la llista d'escenes del build?");
            return;
        }
        Obtenir().StartCoroutine(Obtenir().Fer(escena));
    }

    public static bool EnCurs => instancia != null && instancia.ocupada;

    static Transicio Obtenir()
    {
        if (instancia != null) return instancia;

        GameObject go = new GameObject("Transicio", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
        DontDestroyOnLoad(go);
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;   // per sobre de tot

        GameObject negre = new GameObject("Negre", typeof(RectTransform), typeof(Image));
        RectTransform rt = (RectTransform)negre.transform;
        rt.SetParent(go.transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        negre.GetComponent<Image>().color = Color.black;

        instancia = go.AddComponent<Transicio>();
        instancia.grup = go.GetComponent<CanvasGroup>();
        instancia.grup.alpha = 0f;
        instancia.grup.blocksRaycasts = false;
        return instancia;
    }

    IEnumerator Fer(string escena)
    {
        if (ocupada) yield break;   // si ja n'hi ha una en marxa, no en comencem una altra
        ocupada = true;
        grup.blocksRaycasts = true;  // durant la fosa no es pot clicar res

        // 1. Fosa a negre (i el so baixa)
        for (float t = 0f; t < duradaFosa; t += Time.unscaledDeltaTime)
        {
            float p = t / duradaFosa;
            grup.alpha = p;
            AudioListener.volume = 1f - p;
            yield return null;
        }
        grup.alpha = 1f;
        AudioListener.volume = 0f;

        // 2. Carreguem l'escena nova
        Time.timeScale = 1f;
        yield return SceneManager.LoadSceneAsync(escena);

        // 3. Tornem a aparèixer (i el so torna)
        for (float t = 0f; t < duradaFosa; t += Time.unscaledDeltaTime)
        {
            float p = t / duradaFosa;
            grup.alpha = 1f - p;
            AudioListener.volume = p;
            yield return null;
        }
        grup.alpha = 0f;
        AudioListener.volume = 1f;
        grup.blocksRaycasts = false;
        ocupada = false;
    }
}
