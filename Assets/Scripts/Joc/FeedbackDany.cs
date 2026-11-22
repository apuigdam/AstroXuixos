using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sacseig de càmera i flaix vermell. Es posa sol a la càmera la primera vegada que es fa servir.
///   FeedbackDany.Cop();                    // quan la nau rep un cop
///   FeedbackDany.Sacsejar(0.15f, 0.1f);    // un sacseig petit (p. ex. una explosió)
///
/// Funciona amb el CameraScript de la càmera: abans que el CameraScript la mogui
/// traiem el desplaçament del sacseig, i després el tornem a posar.
/// </summary>
[DefaultExecutionOrder(100)]   // el LateUpdate va DESPRÉS del CameraScript
public class FeedbackDany : MonoBehaviour
{
    const float DuradaFlaix = 0.35f;

    private static FeedbackDany instancia;

    private float tempsSacseig, duradaSacseig, amplitud;
    private Vector3 desplacament;
    private Image flaix;
    private float tempsFlaix;

    static FeedbackDany Obtenir()
    {
        if (instancia != null) return instancia;
        Camera cam = Camera.main;
        if (cam == null) return null;
        instancia = cam.GetComponent<FeedbackDany>();
        if (instancia == null) instancia = cam.gameObject.AddComponent<FeedbackDany>();
        return instancia;
    }

    public static void Sacsejar(float durada, float forca)
    {
        FeedbackDany f = Obtenir();
        if (f == null) return;
        // Si ja n'hi ha un de més fort en marxa, no el fem més fluix
        float forcaActual = f.duradaSacseig > 0f ? f.amplitud * (f.tempsSacseig / f.duradaSacseig) : 0f;
        if (forca < forcaActual) return;
        f.duradaSacseig = durada;
        f.tempsSacseig = durada;
        f.amplitud = forca;
    }

    /// <summary>La nau ha rebut un cop: sacseig fort i flaix vermell.</summary>
    public static void Cop()
    {
        Sacsejar(0.35f, 0.35f);
        FeedbackDany f = Obtenir();
        if (f != null) f.FlaixVermell();
    }

    void Update()
    {
        // Traiem el sacseig del frame anterior abans que el CameraScript mogui la càmera
        transform.position -= desplacament;
        desplacament = Vector3.zero;
    }

    void LateUpdate()
    {
        // Sacseig (s'atura si el joc està en pausa)
        if (tempsSacseig > 0f && Time.timeScale > 0f)
        {
            tempsSacseig -= Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(tempsSacseig / duradaSacseig);
            desplacament = (Vector3)(Random.insideUnitCircle * amplitud * p);
            transform.position += desplacament;
        }

        // Flaix vermell que es fon
        if (flaix != null)
        {
            if (tempsFlaix > 0f) tempsFlaix -= Time.unscaledDeltaTime;
            Color c = flaix.color;
            c.a = 0.35f * Mathf.Clamp01(tempsFlaix / DuradaFlaix);
            flaix.color = c;
        }
    }

    void FlaixVermell()
    {
        if (flaix == null)
        {
            GameObject go = new GameObject("FlaixDany", typeof(RectTransform), typeof(Canvas), typeof(Image));
            go.transform.SetParent(transform, false);
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;   // per sobre del HUD, per sota de la pausa
            flaix = go.GetComponent<Image>();
            flaix.color = new Color(1f, 0.1f, 0.1f, 0f);
            flaix.raycastTarget = false;
        }
        tempsFlaix = DuradaFlaix;
    }

    void OnDestroy()
    {
        if (instancia == this) instancia = null;
    }
}
