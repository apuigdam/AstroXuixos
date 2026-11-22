using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Crèdits finals que es desplacen per la pantalla quan el vídeo final es queda en negre.
/// - ESC durant el vídeo: salta directament als crèdits.
/// - ESC durant els crèdits: torna al menú.
/// - Quan els crèdits acaben, torna al menú.
/// Els crèdits són grups (títol + noms) fills de "contingut": els pots editar, afegir o esborrar
/// a la Hierarchy en ordre de lectura (el primer grup de la llista és el primer que surt).
/// </summary>
public class CreditsFinals : MonoBehaviour
{
    public enum Direccio { DaltABaix, BaixADalt }

    [Header("Vídeo")]
    public VideoPlayer video;
    [Tooltip("Segon del vídeo en què comencen els crèdits (el vídeo es queda negre als 22,5 s)")]
    public float segonInici = 22.5f;

    [Header("Crèdits")]
    public RectTransform contingut;
    public Direccio direccio = Direccio.DaltABaix;
    [Tooltip("Només de dalt a baix: si està marcat, el primer grup de la llista és el primer que entra (i queda a sota de tot). Si no, el bloc baixa en l'ordre normal de lectura.")]
    public bool invertirOrdre = false;
    [Tooltip("Velocitat en píxels per segon (referència 1920x1080)")]
    public float velocitat = 110f;
    [Tooltip("Segons d'espera quan els crèdits ja han sortit de la pantalla")]
    public float esperaFinal = 1f;

    [Header("Resultats de la partida (al moment quiet del vídeo)")]
    public bool mostrarResultats = true;
    public float segonIniciResultats = 15.3f;
    public float segonFiResultats = 20.8f;

    [Header("En acabar")]
    public string escenaFinal = "Menu";
    public bool esPotSaltar = true;

    private bool enMarxa = false;
    private bool acabat = false;
    private bool canviant = false;
    private float alturaPantalla;
    private float alturaContingut;
    private float tempsAcabat;
    private CanvasGroup grupResultats;

    void Start()
    {
        if (contingut == null)
        {
            Debug.LogError("[Credits] Falta assignar el 'contingut' a l'Inspector.");
            enabled = false;
            return;
        }

        // Opcional: de dalt a baix, si volem que el primer grup entri primer, ha d'estar a la
        // part de baix del bloc. Per això invertim l'ordre dels grups
        // (dins de cada grup l'ordre no canvia: el títol continua a sobre dels noms).
        if (direccio == Direccio.DaltABaix && invertirOrdre)
        {
            int n = contingut.childCount;
            for (int i = 0; i < n; i++)
                contingut.GetChild(n - 1).SetSiblingIndex(i);
        }

        contingut.gameObject.SetActive(false);

        if (mostrarResultats) CrearResultats();

        if (video != null)
            video.loopPointReached += VideoAcabat;
    }

    void OnDestroy()
    {
        if (video != null)
            video.loopPointReached -= VideoAcabat;
    }

    void Update()
    {
        // Resultats: apareixen i desapareixen poc a poc mentre el vídeo està quiet
        if (grupResultats != null)
        {
            float a = 0f;
            if (!enMarxa && video != null)
            {
                float t = (float)video.time;
                a = Mathf.Clamp01((t - segonIniciResultats) / 0.6f) * Mathf.Clamp01((segonFiResultats - t) / 0.6f);
            }
            grupResultats.alpha = a;
        }

        // Comencem quan el vídeo arriba al segon indicat (o si no hi ha vídeo)
        if (!enMarxa && (video == null || video.time >= segonInici))
            Comencar();

        if (esPotSaltar && EscPremut())
        {
            if (!enMarxa) Comencar();              // durant el vídeo: anem als crèdits
            else AnarAEscenaFinal();               // durant els crèdits: sortim
        }

        if (!enMarxa) return;

        if (!acabat)
        {
            float pas = velocitat * Time.deltaTime;
            Vector2 pos = contingut.anchoredPosition;

            if (direccio == Direccio.DaltABaix)
            {
                pos.y -= pas;
                // Ha sortit del tot per sota?
                if (pos.y < -(alturaPantalla + alturaContingut)) Acabar();
            }
            else
            {
                pos.y += pas;
                // Ha sortit del tot per dalt?
                if (pos.y > alturaPantalla + alturaContingut) Acabar();
            }

            contingut.anchoredPosition = pos;
        }
        else if (Time.time - tempsAcabat >= esperaFinal)
        {
            AnarAEscenaFinal();
        }
    }

    private void VideoAcabat(VideoPlayer vp)
    {
        if (!enMarxa) Comencar();
    }

    private void Comencar()
    {
        if (enMarxa) return;
        enMarxa = true;

        // Si hem saltat el vídeo, l'aturem perquè es vegi la pantalla negra
        if (video != null && video.time < segonInici)
            video.Stop();

        contingut.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contingut);

        RectTransform pare = (RectTransform)contingut.parent;
        alturaPantalla = pare.rect.height;
        alturaContingut = contingut.rect.height;

        // Col·loquem el bloc just fora de la pantalla, pel costat on entra
        if (direccio == Direccio.DaltABaix)
        {
            contingut.anchorMin = contingut.anchorMax = new Vector2(0.5f, 1f); // ancorat a dalt
            contingut.pivot = new Vector2(0.5f, 0f);                          // la seva vora de baix
        }
        else
        {
            contingut.anchorMin = contingut.anchorMax = new Vector2(0.5f, 0f); // ancorat a baix
            contingut.pivot = new Vector2(0.5f, 1f);                          // la seva vora de dalt
        }
        contingut.anchoredPosition = Vector2.zero;
    }

    private void Acabar()
    {
        acabat = true;
        tempsAcabat = Time.time;
    }

    private void AnarAEscenaFinal()
    {
        if (canviant) return;
        canviant = true;
        Transicio.CarregarEscena(escenaFinal);
    }

    // Punts i xuixos de la partida (ResultatsPartida els guarda el GameManager en acabar)
    private void CrearResultats()
    {
        GameObject go = new GameObject("Resultats", typeof(RectTransform), typeof(CanvasGroup));
        go.layer = gameObject.layer;
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        grupResultats = go.GetComponent<CanvasGroup>();
        grupResultats.alpha = 0f;

        Font font = Resources.Load<Font>("Fonts/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        CrearTextResultat(rt, font, "PUNTS  " + ResultatsPartida.Punts.ToString("0000000"), 46, new Color32(255, 200, 120, 255), -190);
        CrearTextResultat(rt, font, "XUIXOS RECUPERATS  " + ResultatsPartida.Xuixos + "/" + ResultatsPartida.XuixosTotals, 30, new Color32(246, 236, 214, 255), -265);
    }

    private static void CrearTextResultat(RectTransform pare, Font font, string text, int mida, Color color, float y)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
        go.layer = pare.gameObject.layer;
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(pare, false);
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = new Vector2(1800, 80);
        Text t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = mida;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;
        Outline o = go.GetComponent<Outline>();
        o.effectColor = new Color(0f, 0f, 0f, 0.9f);
        o.effectDistance = new Vector2(4, -4);
    }

    private bool EscPremut()
    {
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool start = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        return esc || start;
    }
}
