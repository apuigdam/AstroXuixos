using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pantalla de GAME OVER (GDD: "la nau queda avariada" i surten els punts).
/// La crea el GameManager per codi quan et quedes sense vides. No cal posar res a l'escena.
/// - Apareix poc a poc després de l'animació de mort de la nau.
/// - Amb ESPAI, ENTER o ESC tornes al menú (o sol, al cap de uns segons).
/// </summary>
public class PantallaGameOver : MonoBehaviour
{
    static readonly Color Vermell = new Color32(255, 90, 90, 255);
    static readonly Color Crema = new Color32(246, 236, 214, 255);
    static readonly Color Taronja = new Color32(255, 200, 120, 255);

    public float retardAparicio = 1f;     // deixem veure com mor la nau
    public float duradaFosa = 0.6f;
    public float segonsAbansDePoderSortir = 1.5f;
    public float segonsMaxim = 8f;        // si no prems res, tornes sol al menú

    private string escenaMenu;
    private CanvasGroup grup;
    private Text textContinuar;
    private Text textRecord;
    private Font font;
    private float temps;
    private bool sortint;

    public static PantallaGameOver Mostrar(int punts, int xuixos, int xuixosTotals, string escenaMenu)
    {
        Sons.AturarMusica();   // para la música del joc
        Sons.GameOver();

        GameObject go = new GameObject("PantallaGameOver", typeof(RectTransform), typeof(Canvas),
                                       typeof(CanvasScaler), typeof(CanvasGroup));
        go.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;   // per sobre del HUD
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        PantallaGameOver p = go.AddComponent<PantallaGameOver>();
        p.escenaMenu = escenaMenu;
        p.Construir(punts, xuixos, xuixosTotals);
        return p;
    }

    void Construir(int punts, int xuixos, int xuixosTotals)
    {
        font = Resources.Load<Font>("Fonts/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        grup = GetComponent<CanvasGroup>();
        grup.alpha = 0f;

        // Fons fosc a tota la pantalla
        GameObject fons = new GameObject("Fons", typeof(RectTransform), typeof(Image));
        fons.layer = gameObject.layer;
        RectTransform rtFons = (RectTransform)fons.transform;
        rtFons.SetParent(transform, false);
        rtFons.anchorMin = Vector2.zero;
        rtFons.anchorMax = Vector2.one;
        rtFons.offsetMin = rtFons.offsetMax = Vector2.zero;
        fons.GetComponent<Image>().color = new Color(0.03f, 0.02f, 0.08f, 0.85f);

        CrearText("GAME OVER", 110, Vermell, 220);
        CrearText("La nau ha quedat avariada", 26, Crema, 90);
        CrearText("PUNTS  " + punts.ToString("000000"), 40, Taronja, -20);
        CrearText("XUIXOS  " + xuixos + "/" + xuixosTotals, 40, Taronja, -90);
        if (ResultatsPartida.NouRecord)
            textRecord = CrearText("NOU RECORD!", 30, new Color32(255, 230, 90, 255), -160);
        else
            CrearText("RECORD  " + ResultatsPartida.Record.ToString("000000"), 26, Crema, -160);
        textContinuar = CrearText("Prem ESPAI per tornar al menú", 24, Crema, -250);
        textContinuar.enabled = false;
    }

    Text CrearText(string contingut, int mida, Color color, float y)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
        go.layer = gameObject.layer;
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = new Vector2(1800, 150);

        Text t = go.GetComponent<Text>();
        t.font = font;
        t.text = contingut;
        t.fontSize = mida;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;

        Outline o = go.GetComponent<Outline>();
        o.effectColor = new Color(0f, 0f, 0f, 0.9f);
        o.effectDistance = new Vector2(4, -4);
        return t;
    }

    void Update()
    {
        temps += Time.unscaledDeltaTime;

        // Aparició progressiva
        grup.alpha = Mathf.Clamp01((temps - retardAparicio) / duradaFosa);

        float tempsVisible = temps - retardAparicio;
        if (tempsVisible < segonsAbansDePoderSortir) return;

        // "NOU RECORD!" batega
        if (textRecord != null)
            textRecord.transform.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.unscaledTime * 6f));

        // "Prem ESPAI..." fa pampallugues
        textContinuar.enabled = Mathf.Repeat(Time.unscaledTime, 1f) < 0.6f;

        if (TeclaPremuda() || tempsVisible >= segonsMaxim)
            TornarAlMenu();
    }

    bool TeclaPremuda()
    {
        Keyboard k = Keyboard.current;
        if (k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.escapeKey.wasPressedThisFrame))
            return true;
        Gamepad g = Gamepad.current;
        return g != null && (g.buttonSouth.wasPressedThisFrame || g.startButton.wasPressedThisFrame);
    }

    void TornarAlMenu()
    {
        if (sortint) return;
        sortint = true;
        Time.timeScale = 1f;
        Transicio.CarregarEscena(escenaMenu);
    }
}
