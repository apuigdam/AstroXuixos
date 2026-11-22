using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fa funcionar el HUD de l'escena Game (el disseny és de l'Arnau, això només el connecta).
/// Va al Canvas del HUD. Llegeix les dades del GameManager cada frame:
///  - Barra de vida: amb 3 vides o més es veu Vida100, amb 2 Vida66 i amb 1 Vida25.
///  - Puntuació: el text "0000000".
///  - A dalt a l'esquerra: xuixos recollits (x/10) i els efectes actius amb els segons que queden.
///  - Al mig: el missatge gran de cada efecte ("TECLES INVERTIDES!"...).
/// Els textos de dalt a l'esquerra i del mig es creen sols si no se n'assigna cap.
/// </summary>
public class HUDJoc : MonoBehaviour
{
    [Header("Barra de vida")]
    public GameObject vida100;
    public GameObject vida66;
    public GameObject vida25;

    [Header("Puntuació")]
    public TMP_Text textPunts;
    public string formatPunts = "0000000";

    [Header("Xuixos i efectes (es creen sols si estan buits)")]
    public Text textInfo;
    public Text textMissatge;
    public float segonsMissatge = 2.2f;

    static readonly Color Taronja = new Color32(255, 200, 120, 255);

    /// <summary>Efecte "HUD?": amaga tot el HUD mentre dura.</summary>
    public static bool Amagat = false;
    private CanvasGroup grup;

    private GameManager gm;
    private float tempsMissatge;
    private Color colorMissatge;
    private Font font;

    void Start()
    {
        Amagat = false;
        grup = GetComponent<CanvasGroup>();
        if (grup == null) grup = gameObject.AddComponent<CanvasGroup>();

        font = Resources.Load<Font>("Fonts/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (textInfo == null)
            textInfo = CrearText("InfoXuixosEfectes", new Vector2(0f, 1f), new Vector2(40, -40), 24, TextAnchor.UpperLeft);
        if (textMissatge == null)
            textMissatge = CrearText("MissatgeEfecte", new Vector2(0.5f, 0.5f), new Vector2(0, 160), 48, TextAnchor.MiddleCenter);

        textInfo.supportRichText = true;
        textInfo.lineSpacing = 1.4f;
        textMissatge.text = "";
    }

    void OnDestroy()
    {
        if (gm != null) gm.MissatgeMostrat -= MostrarMissatge;
    }

    void Update()
    {
        // El GameManager es crea sol en començar la partida: l'agafem quan ja existeix
        if (gm == null)
        {
            gm = GameManager.Instance;
            if (gm == null) return;
            gm.MissatgeMostrat += MostrarMissatge;
        }

        grup.alpha = Amagat ? 0f : 1f;

        // Barra de vida
        int vides = gm.Vides;
        if (vida100 != null) vida100.SetActive(vides >= 3);
        if (vida66 != null) vida66.SetActive(vides == 2);
        if (vida25 != null) vida25.SetActive(vides == 1);

        // Punts
        if (textPunts != null)
            textPunts.text = gm.Punts.ToString(formatPunts) + (gm.MultiplicadorPunts > 1 ? " x" + gm.MultiplicadorPunts : "");

        // Xuixos i efectes actius
        StringBuilder sb = new StringBuilder();
        sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(Taronja)).Append(">XUIXOS ")
          .Append(gm.XuixosRecollits).Append("/").Append(gm.XuixosTotals).Append("</color>\n");
        if (gm.Efectes != null)
        {
            foreach (GestorEfectes.EfecteActiu a in gm.Efectes.Actius)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(GestorEfectes.ColorDe(a.efecte.Tipus))).Append(">")
                  .Append(a.efecte.Nom).Append(" ").Append(Mathf.CeilToInt(a.tempsRestant)).Append("s</color>\n");
            }
        }
        textInfo.text = sb.ToString();

        // Missatge gran que es fon
        if (tempsMissatge > 0f)
        {
            tempsMissatge -= Time.unscaledDeltaTime;
            Color c = colorMissatge;
            c.a = Mathf.Clamp01(tempsMissatge / 0.5f);
            textMissatge.color = c;
            if (tempsMissatge <= 0f) textMissatge.text = "";
        }
    }

    void MostrarMissatge(string text, Color color)
    {
        if (textMissatge == null) return;
        textMissatge.text = text;
        colorMissatge = color;
        tempsMissatge = segonsMissatge;
    }

    Text CrearText(string nom, Vector2 ancora, Vector2 posicio, int mida, TextAnchor alineacio)
    {
        GameObject go = new GameObject(nom, typeof(RectTransform), typeof(Text), typeof(Outline));
        go.layer = gameObject.layer;
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        rt.anchorMin = rt.anchorMax = ancora;
        rt.pivot = ancora;
        rt.anchoredPosition = posicio;
        rt.sizeDelta = new Vector2(1000, 300);

        Text t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = mida;
        t.color = Color.white;
        t.alignment = alineacio;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;

        Outline o = go.GetComponent<Outline>();
        o.effectColor = new Color(0f, 0f, 0f, 0.85f);
        o.effectDistance = new Vector2(3, -3);
        return t;
    }
}
