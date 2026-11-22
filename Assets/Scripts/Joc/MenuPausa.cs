using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Menú de pausa de la partida (GDD). L'afegeix el GameManager sol, no cal posar res a l'escena.
/// - ESC (o Start al comandament) pausa i torna a continuar.
/// - Mentre està en pausa s'atura tot: meteorits, efectes, punts, música i dispars.
/// - Botons: CONTINUAR i TORNAR AL MENÚ (amb teclat, ratolí o comandament).
/// </summary>
public class MenuPausa : MonoBehaviour
{
    static readonly Color Taronja = new Color32(255, 200, 120, 255);
    static readonly Color Crema = new Color32(246, 236, 214, 255);
    static readonly Color TextBoto = new Color32(40, 22, 16, 255);

    public string escenaMenu = "Menu";

    public bool EnPausa { get; private set; }

    private GameObject pantalla;
    private GameObject botoContinuar;
    private float escalaAbans = 1f;
    private Font font;

    void Start()
    {
        font = Resources.Load<Font>("Fonts/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Construir();
        pantalla.SetActive(false);
    }

    void Update()
    {
        bool tecla = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                     (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
        if (!tecla) return;

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.PartidaAcabada || Transicio.EnCurs) return;

        if (EnPausa) Continuar();
        else Pausar();
    }

    public void Pausar()
    {
        if (EnPausa) return;
        EnPausa = true;

        escalaAbans = Time.timeScale;      // per si hi havia la Càmera lenta
        Time.timeScale = 0f;
        AudioListener.pause = true;        // para la música i els sons del joc

        GameManager gm = GameManager.Instance;
        if (gm != null && gm.Jugador != null) gm.Jugador.enabled = false;   // que no dispari amb ESPAI

        pantalla.SetActive(true);
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(botoContinuar);
        }
    }

    public void Continuar()
    {
        if (!EnPausa) return;
        EnPausa = false;
        Sons.ClicBoto();

        pantalla.SetActive(false);
        AudioListener.pause = false;
        Time.timeScale = escalaAbans;

        GameManager gm = GameManager.Instance;
        if (gm != null && gm.Jugador != null && !gm.PartidaAcabada) gm.Jugador.enabled = true;
    }

    public void TornarAlMenu()
    {
        Sons.ClicBoto();
        AudioListener.pause = false;
        Time.fixedDeltaTime = 0.02f;       // per si hi havia la Càmera lenta
        Transicio.CarregarEscena(escenaMenu);
    }

    void OnDestroy()
    {
        // Si se surt de l'escena en pausa, que el so no quedi parat
        if (EnPausa) AudioListener.pause = false;
    }

    // ================== CONSTRUCCIÓ DE LA PANTALLA ==================

    void Construir()
    {
        pantalla = new GameObject("MenuPausa", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        pantalla.layer = LayerMask.NameToLayer("UI");
        pantalla.transform.SetParent(transform, false);
        Canvas canvas = pantalla.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;   // per sobre del HUD, per sota de les transicions
        CanvasScaler scaler = pantalla.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Fons fosc
        RectTransform fons = CrearRect("Fons", pantalla.transform);
        fons.anchorMin = Vector2.zero;
        fons.anchorMax = Vector2.one;
        fons.offsetMin = fons.offsetMax = Vector2.zero;
        fons.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.02f, 0.08f, 0.8f);

        CrearText("PAUSA", 96, Taronja, new Vector2(0, 260), pantalla.transform);
        CrearText("FLETXES  moure      ESPAI  disparar", 22, Crema, new Vector2(0, 150), pantalla.transform);

        botoContinuar = CrearBoto("CONTINUAR", new Vector2(0, 10), Continuar);
        GameObject botoMenu = CrearBoto("TORNAR AL MENU", new Vector2(0, -130), TornarAlMenu);

        // Navegació amunt/avall entre els dos botons
        Navigation n1 = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = botoMenu.GetComponent<Button>(), selectOnUp = botoMenu.GetComponent<Button>() };
        Navigation n2 = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = botoContinuar.GetComponent<Button>(), selectOnDown = botoContinuar.GetComponent<Button>() };
        botoContinuar.GetComponent<Button>().navigation = n1;
        botoMenu.GetComponent<Button>().navigation = n2;

        CrearText("ESC per continuar", 18, new Color(1f, 1f, 1f, 0.6f), new Vector2(0, -280), pantalla.transform);
    }

    GameObject CrearBoto(string text, Vector2 pos, UnityEngine.Events.UnityAction accio)
    {
        RectTransform rt = CrearRect("Boto_" + text, pantalla.transform);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(620, 100);

        Image img = rt.gameObject.AddComponent<Image>();
        img.color = Color.white;

        Button boto = rt.gameObject.AddComponent<Button>();
        boto.targetGraphic = img;
        ColorBlock colors = boto.colors;
        colors.normalColor = Crema;
        colors.highlightedColor = Taronja;
        colors.selectedColor = Taronja;
        colors.pressedColor = new Color(0.9f, 0.62f, 0.4f);
        colors.fadeDuration = 0.05f;
        boto.colors = colors;
        boto.onClick.AddListener(accio);

        Outline vora = rt.gameObject.AddComponent<Outline>();
        vora.effectColor = new Color32(34, 18, 14, 255);
        vora.effectDistance = new Vector2(6, -6);

        Text etiqueta = CrearText(text, 36, TextBoto, Vector2.zero, rt);
        RectTransform rtText = etiqueta.rectTransform;
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.offsetMin = rtText.offsetMax = Vector2.zero;
        etiqueta.GetComponent<Outline>().enabled = false;

        // El mateix efecte que els botons del menú principal (creix en seleccionar-lo i fa so)
        EfecteBoto efecte = rt.gameObject.AddComponent<EfecteBoto>();
        efecte.etiqueta = etiqueta;
        efecte.colorNormal = TextBoto;
        efecte.colorSeleccionat = TextBoto;

        return rt.gameObject;
    }

    RectTransform CrearRect(string nom, Transform pare)
    {
        GameObject go = new GameObject(nom, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(pare, false);
        return rt;
    }

    Text CrearText(string contingut, int mida, Color color, Vector2 pos, Transform pare)
    {
        RectTransform rt = CrearRect("Text", pare);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(1600, 120);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = contingut;
        t.fontSize = mida;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;
        Outline o = rt.gameObject.AddComponent<Outline>();
        o.effectColor = new Color(0f, 0f, 0f, 0.9f);
        o.effectDistance = new Vector2(4, -4);
        return t;
    }
}
