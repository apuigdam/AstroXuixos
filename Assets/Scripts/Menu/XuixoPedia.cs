using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// XUIXOPEDIA: pàgina del menú que explica tots els efectes dels xuixos.
/// La crea el MenuPrincipal sol en començar: posa el botó XUIXOPEDIA sota CONTROLS
/// i construeix la pàgina (amagada fins que la obres).
/// Els noms, tipus, durades i descripcions surten dels mateixos efectes
/// (EfectesXuixos.cs), així que si n'afegiu un de nou, surt aquí sol.
/// </summary>
public class XuixoPedia : MonoBehaviour
{
    public static bool Oberta { get; private set; }

    static readonly Color Taronja = new Color32(255, 200, 120, 255);
    static readonly Color Crema = new Color32(246, 236, 214, 255);

    const int Columnes = 2;
    const float AmpleColumna = 780f;
    const float AltFila = 86f;

    private GameObject pagina;
    private GameObject botoObrir;
    private GameObject botoTornar;
    private Font font;

    public void Iniciar(GameObject botoControls, GameObject botoTornarControls, GameObject panellControls)
    {
        Oberta = false;
        if (botoControls == null) return;

        Canvas canvas = botoControls.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;

        font = Resources.Load<Font>("Fonts/PressStart2P-Regular");

        CrearBotoMenu(botoControls);
        CrearPagina(canvas.transform, botoTornarControls, panellControls);
        pagina.SetActive(false);
    }

    // ---------- Botó XUIXOPEDIA al menú ----------

    void CrearBotoMenu(GameObject botoControls)
    {
        // Copiem el botó CONTROLS perquè quedi igual (imatge, colors, so i animació)
        Transform pare = botoControls.transform.parent;
        botoObrir = Instantiate(botoControls, pare);
        botoObrir.name = "BotoXuixopedia";
        botoObrir.transform.SetSiblingIndex(botoControls.transform.GetSiblingIndex() + 1);

        Text etiqueta = botoObrir.GetComponentInChildren<Text>();
        if (etiqueta != null) etiqueta.text = "XUIXOPEDIA";

        Button b = botoObrir.GetComponent<Button>();
        if (b != null)
        {
            b.onClick = new Button.ButtonClickedEvent();   // treu el clic de CONTROLS que s'ha copiat
            b.onClick.AddListener(Obrir);
        }

        // Ara hi ha 4 botons: els fem una mica més baixos perquè hi càpiguen
        VerticalLayoutGroup layout = pare.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 18f;
            foreach (Transform fill in pare)
            {
                LayoutElement le = fill.GetComponent<LayoutElement>();
                if (le != null) le.preferredHeight = 96f;
            }
            RectTransform rtPare = (RectTransform)pare;
            int n = pare.childCount;
            rtPare.sizeDelta = new Vector2(rtPare.sizeDelta.x, Mathf.Max(rtPare.sizeDelta.y, n * 96f + (n - 1) * 18f));
        }
    }

    // ---------- Pàgina ----------

    void CrearPagina(Transform canvas, GameObject botoTornarControls, GameObject panellControls)
    {
        // Fons fosc a tota la pantalla (també bloqueja els clics als botons de darrere)
        pagina = new GameObject("PaginaXuixopedia", typeof(RectTransform), typeof(Image));
        RectTransform rtPagina = (RectTransform)pagina.transform;
        rtPagina.SetParent(canvas, false);
        rtPagina.SetAsLastSibling();
        Estirar(rtPagina);
        pagina.GetComponent<Image>().color = new Color(0.02f, 0.01f, 0.05f, 0.95f);

        // Caixa amb el mateix marc que el panell de controls
        GameObject caixaGO = new GameObject("Caixa", typeof(RectTransform), typeof(Image));
        RectTransform caixa = (RectTransform)caixaGO.transform;
        caixa.SetParent(rtPagina, false);
        caixa.sizeDelta = new Vector2(1760, 990);
        Image imgCaixa = caixaGO.GetComponent<Image>();
        Image marc = null;
        if (panellControls != null)
        {
            Transform c = panellControls.transform.Find("Caixa");
            if (c != null) marc = c.GetComponent<Image>();
        }
        if (marc != null && marc.sprite != null)
        {
            imgCaixa.sprite = marc.sprite;
            imgCaixa.type = Image.Type.Sliced;
            imgCaixa.color = marc.color;
        }
        else
        {
            imgCaixa.color = new Color(0.1f, 0.07f, 0.16f, 0.95f);
        }

        // Títol i explicació
        Text titol = CrearText("Titol", caixa, "XUIXOPEDIA", 52, Taronja, TextAnchor.MiddleCenter);
        Posar(titol.rectTransform, new Vector2(0, -45), new Vector2(1600, 64));

        Text intro = CrearText("Intro", caixa,
            "Cada xuixo que recuperes dona +1000 punts i un efecte a l'atzar.\nCada efecte pot sortir com a molt 2 vegades per partida.",
            16, new Color(Crema.r, Crema.g, Crema.b, 0.8f), TextAnchor.MiddleCenter);
        intro.lineSpacing = 1.4f;
        Posar(intro.rectTransform, new Vector2(0, -118), new Vector2(1600, 50));

        // Fitxes dels efectes: primer els bons, després els arriscats i al final els dolents
        List<EfecteXuixo> efectes = GestorEfectes.TotsElsEfectes();
        List<EfecteXuixo> ordenats = new List<EfecteXuixo>();
        foreach (TipusEfecte t in new[] { TipusEfecte.Buff, TipusEfecte.Neutre, TipusEfecte.Debuff })
            foreach (EfecteXuixo e in efectes)
                if (TipusPedia(e) == t) ordenats.Add(e);

        for (int i = 0; i < ordenats.Count; i++)
        {
            int col = i % Columnes;
            int fila = i / Columnes;
            float x = (col - (Columnes - 1) / 2f) * (AmpleColumna + 40f);
            CrearFitxa(caixa, ordenats[i], new Vector2(x, -190f - fila * AltFila));
        }

        // Llegenda de colors
        Text llegenda = CrearText("Llegenda", caixa,
            "<color=#80FF80>VERD</color> t'ajuda    <color=#FFD95A>GROC</color> arriscat    <color=#FF6666>VERMELL</color> et complica la vida",
            14, Crema, TextAnchor.MiddleCenter);
        llegenda.supportRichText = true;
        RectTransform rtLlegenda = llegenda.rectTransform;
        rtLlegenda.anchorMin = rtLlegenda.anchorMax = rtLlegenda.pivot = new Vector2(0.5f, 0f);
        rtLlegenda.anchoredPosition = new Vector2(0, 160);
        rtLlegenda.sizeDelta = new Vector2(1600, 30);

        // Botó TORNAR (copia del de la pantalla de controls, perquè quedi igual)
        if (botoTornarControls != null)
        {
            botoTornar = Instantiate(botoTornarControls, caixa);
            botoTornar.name = "BotoTornar";
            botoTornar.SetActive(true);
            RectTransform rt = (RectTransform)botoTornar.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 45);
            rt.sizeDelta = new Vector2(420, 96);
            Button b = botoTornar.GetComponent<Button>();
            if (b != null)
            {
                b.onClick = new Button.ButtonClickedEvent();
                b.onClick.AddListener(Tancar);
            }
        }
    }

    void CrearFitxa(RectTransform caixa, EfecteXuixo e, Vector2 posicio)
    {
        TipusEfecte tipus = TipusPedia(e);
        Color color = GestorEfectes.ColorDe(tipus);

        GameObject go = new GameObject("Fitxa_" + e.Nom, typeof(RectTransform));
        RectTransform fitxa = (RectTransform)go.transform;
        fitxa.SetParent(caixa, false);
        fitxa.anchorMin = fitxa.anchorMax = fitxa.pivot = new Vector2(0.5f, 1f);
        fitxa.anchoredPosition = posicio;
        fitxa.sizeDelta = new Vector2(AmpleColumna, AltFila - 6f);

        // Franja de color a l'esquerra
        GameObject franja = new GameObject("Franja", typeof(RectTransform), typeof(Image));
        RectTransform rtFranja = (RectTransform)franja.transform;
        rtFranja.SetParent(fitxa, false);
        rtFranja.anchorMin = new Vector2(0f, 0f);
        rtFranja.anchorMax = new Vector2(0f, 1f);
        rtFranja.pivot = new Vector2(0f, 0.5f);
        rtFranja.anchoredPosition = Vector2.zero;
        rtFranja.sizeDelta = new Vector2(8f, -8f);
        Image imgFranja = franja.GetComponent<Image>();
        imgFranja.color = color;
        imgFranja.raycastTarget = false;

        // Nom (esquerra) i etiqueta de tipus + durada (dreta)
        Text nom = CrearText("Nom", fitxa, e.Nom, 20, color, TextAnchor.UpperLeft);
        Dalt(nom.rectTransform, 24f, 0f, AmpleColumna - 24f, 26f);

        Text etiqueta = CrearText("Etiqueta", fitxa, Etiqueta(e), 14, new Color(color.r, color.g, color.b, 0.8f), TextAnchor.UpperRight);
        Dalt(etiqueta.rectTransform, 24f, -3f, AmpleColumna - 24f, 22f);

        // Descripció
        Text desc = CrearText("Descripcio", fitxa, e.Descripcio, 14, Crema, TextAnchor.UpperLeft);
        desc.horizontalOverflow = HorizontalWrapMode.Wrap;
        desc.lineSpacing = 1.35f;
        Dalt(desc.rectTransform, 24f, -34f, AmpleColumna - 24f, 46f);
    }

    // El placebo es fa passar per bo al joc, però a la XUIXOPEDIA el posem com a "???"
    static TipusEfecte TipusPedia(EfecteXuixo e) => e is EfectePlacebo ? TipusEfecte.Neutre : e.Tipus;

    static string Etiqueta(EfecteXuixo e)
    {
        if (e is EfectePlacebo) return "???";
        string tipus = e.Tipus == TipusEfecte.Buff ? "BUFF" : e.Tipus == TipusEfecte.Debuff ? "DEBUFF" : "RISC";
        string durada = e.Durada > 0f ? Mathf.RoundToInt(e.Durada) + " s" : "AL MOMENT";
        return tipus + "  " + durada;
    }

    // ---------- Obrir i tancar ----------

    public void Obrir()
    {
        if (pagina == null) return;
        Sons.ClicBoto();
        pagina.SetActive(true);
        Oberta = true;
        Seleccionar(botoTornar);
    }

    public void Tancar()
    {
        if (pagina == null) return;
        Sons.ClicBoto();
        pagina.SetActive(false);
        Oberta = false;
        Seleccionar(botoObrir);
    }

    void Update()
    {
        if (!Oberta) return;

        // ESC (o el botó B del comandament) tanca la pàgina
        bool tecla = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool comandament = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        if (tecla || comandament)
        {
            Tancar();
            return;
        }

        // Si s'ha perdut la selecció (clicant al buit), tornem a seleccionar TORNAR
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            Seleccionar(botoTornar);
    }

    void OnDestroy()
    {
        Oberta = false;
    }

    // ---------- Ajudes ----------

    Text CrearText(string nom, Transform pare, string contingut, int mida, Color color, TextAnchor alineacio)
    {
        GameObject go = new GameObject(nom, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(pare, false);
        Text t = go.GetComponent<Text>();
        t.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = contingut;
        t.fontSize = mida;
        t.color = color;
        t.alignment = alineacio;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = false;
        t.raycastTarget = false;
        return t;
    }

    // Ancorat a dalt al mig de la caixa
    static void Posar(RectTransform rt, Vector2 posicio, Vector2 mida)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = posicio;
        rt.sizeDelta = mida;
    }

    // Ancorat a dalt a l'esquerra de la fitxa
    static void Dalt(RectTransform rt, float x, float y, float ample, float alt)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(ample, alt);
    }

    static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Seleccionar(GameObject objecte)
    {
        if (objecte == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(objecte);
    }
}
