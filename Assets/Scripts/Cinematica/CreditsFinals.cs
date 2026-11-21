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
    [Tooltip("Velocitat en píxels per segon (referència 1920x1080)")]
    public float velocitat = 110f;
    [Tooltip("Segons d'espera quan els crèdits ja han sortit de la pantalla")]
    public float esperaFinal = 1f;

    [Header("En acabar")]
    public string escenaFinal = "Menu";
    public bool esPotSaltar = true;

    private bool enMarxa = false;
    private bool acabat = false;
    private bool canviant = false;
    private float alturaPantalla;
    private float alturaContingut;
    private float tempsAcabat;

    void Start()
    {
        if (contingut == null)
        {
            Debug.LogError("[Credits] Falta assignar el 'contingut' a l'Inspector.");
            enabled = false;
            return;
        }

        // De dalt a baix: el primer grup ha d'entrar primer, i com que entra per dalt,
        // ha d'estar a la part de baix del bloc. Per això invertim l'ordre dels grups
        // (dins de cada grup l'ordre no canvia: el títol continua a sobre dels noms).
        if (direccio == Direccio.DaltABaix)
        {
            int n = contingut.childCount;
            for (int i = 0; i < n; i++)
                contingut.GetChild(n - 1).SetSiblingIndex(i);
        }

        contingut.gameObject.SetActive(false);

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

        if (!Application.CanStreamedLevelBeLoaded(escenaFinal))
        {
            Debug.LogError("[Credits] No es pot carregar l'escena '" + escenaFinal + "'.");
            return;
        }
        SceneManager.LoadScene(escenaFinal);
    }

    private bool EscPremut()
    {
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool start = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        return esc || start;
    }
}
