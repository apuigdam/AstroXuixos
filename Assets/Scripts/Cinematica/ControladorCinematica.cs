using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Posa'l al mateix GameObject que el Video Player de la cinemàtica.
/// - Si "textHistoria" no és buit, primer surt el text de la història (efecte màquina d'escriure)
///   i el vídeo comença quan prems ESPAI / ENTER (o sol al cap d'uns segons).
/// - Quan el vídeo s'acaba, carrega l'escena següent amb fosa a negre.
/// - Si prems ESC (o Start al comandament), se salta tot.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class ControladorCinematica : MonoBehaviour
{
    [Tooltip("Escena que es carrega quan acaba (o se salta) el vídeo")]
    public string escenaSeguent = "Game";

    [Tooltip("Permetre saltar la cinemàtica amb ESC")]
    public bool esPotSaltar = true;

    [Header("Text d'història abans del vídeo (buit = sense text)")]
    [TextArea(3, 8)]
    public string textHistoria =
        "El xuixo de Girona és el postre més buscat de l'univers pels seus efectes exòtics.\n\n" +
        "Un traficant espacial s'ha estavellat contra un meteorit i ha perdut tot el carregament a l'espai.\n\n" +
        "Desplega la nau secreta de la Catedral de Girona, destrossa els asteroides i recupera'ls...\n\n" +
        "si sobrevius als seus canvis de regles!";
    public float lletresPerSegon = 45f;
    public float segonsMaximText = 14f;

    private VideoPlayer video;
    private bool canviant = false;
    private bool mostrantHistoria = false;

    void Awake()
    {
        video = GetComponent<VideoPlayer>();
        video.isLooping = false;               // si fes bucle, no s'acabaria mai

        if (!string.IsNullOrEmpty(textHistoria))
        {
            // El vídeo no comença fins que s'acaba el text
            video.playOnAwake = false;
            video.Stop();
            mostrantHistoria = true;
        }
    }

    void Start()
    {
        if (mostrantHistoria) StartCoroutine(Historia());
    }

    void OnEnable()
    {
        video = GetComponent<VideoPlayer>();
        video.loopPointReached += VideoAcabat;
        video.errorReceived += ErrorVideo;
    }

    void OnDisable()
    {
        video.loopPointReached -= VideoAcabat;
        video.errorReceived -= ErrorVideo;
    }

    void Update()
    {
        if (!esPotSaltar) return;

        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool start = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

        if (esc || start)
            AnarEscenaSeguent();
    }

    private void VideoAcabat(VideoPlayer vp)
    {
        AnarEscenaSeguent();
    }

    private void ErrorVideo(VideoPlayer vp, string missatge)
    {
        Debug.LogWarning("[Cinematica] Error amb el vídeo: " + missatge + ". Passem a l'escena següent.");
        AnarEscenaSeguent();
    }

    private void AnarEscenaSeguent()
    {
        if (canviant) return;   // evita carregar l'escena dues vegades
        canviant = true;
        // Fosa a negre cap a l'escena següent (el vídeo es queda a l'últim fotograma mentre es fon)
        Transicio.CarregarEscena(escenaSeguent);
    }

    // ================== TEXT D'HISTÒRIA ==================

    IEnumerator Historia()
    {
        // Pantalla negra amb el text
        GameObject go = new GameObject("Historia", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        CanvasGroup grup = go.GetComponent<CanvasGroup>();

        Image fons = CrearFill<Image>("Fons", go.transform);
        fons.color = Color.black;
        RectTransform rtFons = fons.rectTransform;
        rtFons.anchorMin = Vector2.zero;
        rtFons.anchorMax = Vector2.one;
        rtFons.offsetMin = rtFons.offsetMax = Vector2.zero;

        Font font = Resources.Load<Font>("Fonts/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Text text = CrearFill<Text>("Text", go.transform);
        text.font = font;
        text.fontSize = 30;
        text.lineSpacing = 1.5f;
        text.color = new Color32(246, 236, 214, 255);
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.rectTransform.sizeDelta = new Vector2(1400, 700);
        text.rectTransform.anchoredPosition = new Vector2(0, 40);
        text.text = "";

        Text avis = CrearFill<Text>("Avis", go.transform);
        avis.font = font;
        avis.fontSize = 20;
        avis.color = new Color32(255, 200, 120, 255);
        avis.alignment = TextAnchor.MiddleCenter;
        avis.horizontalOverflow = HorizontalWrapMode.Overflow;
        avis.rectTransform.sizeDelta = new Vector2(1400, 60);
        avis.rectTransform.anchoredPosition = new Vector2(0, -430);
        avis.text = "Prem ESPAI per continuar";
        avis.enabled = false;

        // Efecte màquina d'escriure (ESPAI el completa de cop)
        float lletres = 0f;
        while (lletres < textHistoria.Length)
        {
            if (Continuar()) break;
            lletres += lletresPerSegon * Time.deltaTime;
            text.text = textHistoria.Substring(0, Mathf.Min(textHistoria.Length, (int)lletres));
            yield return null;
        }
        text.text = textHistoria;
        yield return null;

        // Esperem que el jugador premi ESPAI (o que passin uns segons)
        float espera = 0f;
        while (!Continuar() && espera < segonsMaximText)
        {
            espera += Time.deltaTime;
            avis.enabled = Mathf.Repeat(Time.time, 1f) < 0.6f;
            yield return null;
        }

        // Comença el vídeo i el text es fon
        mostrantHistoria = false;
        if (!canviant) video.Play();
        for (float t = 0f; t < 0.8f; t += Time.deltaTime)
        {
            grup.alpha = 1f - t / 0.8f;
            yield return null;
        }
        Destroy(go);
    }

    bool Continuar()
    {
        Keyboard k = Keyboard.current;
        if (k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame)) return true;
        Mouse m = Mouse.current;
        if (m != null && m.leftButton.wasPressedThisFrame) return true;
        Gamepad g = Gamepad.current;
        return g != null && g.buttonSouth.wasPressedThisFrame;
    }

    static T CrearFill<T>(string nom, Transform pare) where T : Component
    {
        GameObject go = new GameObject(nom, typeof(RectTransform));
        go.transform.SetParent(pare, false);
        return go.AddComponent<T>();
    }
}
