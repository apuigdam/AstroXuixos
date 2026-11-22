using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el menú principal: botons Jugar, Controls i Sortir.
/// Els botons criden aquests mètodes públics des del seu OnClick().
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    [Header("Escena a carregar quan es prem JUGAR")]
    [Tooltip("Ha d'estar afegida a File > Build Profiles > Scene List")]
    public string escenaJugar = "CinematicaInicial";

    [Header("Referències de la UI")]
    public GameObject panellControls;
    public GameObject botoJugar;
    public GameObject botoControls;
    public GameObject botoTornar;

    private bool carregant = false;

    void Start()
    {
        // Per si tornem al menú des d'una pausa amb el temps aturat
        Time.timeScale = 1f;

        if (panellControls != null)
            panellControls.SetActive(false);

        // Deixem el botó JUGAR seleccionat perquè funcioni amb teclat i comandament
        Seleccionar(botoJugar);

        // Música del menú (surt de BibliotecaSons)
        if (Sons.Biblioteca != null && Sons.Biblioteca.musicaMenu != null)
        {
            AudioSource musica = gameObject.AddComponent<AudioSource>();
            musica.clip = Sons.Biblioteca.musicaMenu;
            musica.loop = true;
            musica.volume = 0.6f;
            musica.Play();
        }
    }

    void Update()
    {
        bool controlsOberts = panellControls != null && panellControls.activeSelf;

        // ESC (o el botó B del comandament) tanca el panell de controls
        if (controlsOberts && EnrerePremut())
            TancarControls();

        // Si s'ha perdut la selecció (p. ex. clicant al buit) i l'usuari fa servir
        // el teclat o el comandament, tornem a seleccionar un botó
        if (EventSystem.current != null
            && EventSystem.current.currentSelectedGameObject == null
            && NavegacioPremuda())
        {
            Seleccionar(controlsOberts ? botoTornar : botoJugar);
        }
    }

    // ---------- Mètodes dels botons ----------

    public void Jugar()
    {
        if (carregant) return;

        if (!Application.CanStreamedLevelBeLoaded(escenaJugar))
        {
            Debug.LogError("[Menu] No es pot carregar l'escena '" + escenaJugar +
                           "'. Afegeix-la a File > Build Profiles > Scene List.");
            return;
        }

        carregant = true;
        Sons.ClicBoto();
        Transicio.CarregarEscena(escenaJugar);
    }

    public void ObrirControls()
    {
        if (panellControls == null) return;
        Sons.ClicBoto();
        panellControls.SetActive(true);
        Seleccionar(botoTornar);
    }

    public void TancarControls()
    {
        if (panellControls == null) return;
        Sons.ClicBoto();
        panellControls.SetActive(false);
        Seleccionar(botoControls);
    }

    public void Sortir()
    {
        Debug.Log("[Menu] Sortint del joc...");
        Sons.ClicBoto();
#if UNITY_EDITOR
        // A l'editor, Application.Quit() no fa res: aturem el mode Play
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Ajudes ----------

    private void Seleccionar(GameObject objecte)
    {
        if (objecte == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(objecte);
    }

    private bool EnrerePremut()
    {
        bool tecla = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool comandament = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        return tecla || comandament;
    }

    private bool NavegacioPremuda()
    {
        Keyboard k = Keyboard.current;
        if (k != null && (k.upArrowKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame ||
                          k.wKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame ||
                          k.tabKey.wasPressedThisFrame))
            return true;

        Gamepad g = Gamepad.current;
        if (g != null && (g.dpad.up.wasPressedThisFrame || g.dpad.down.wasPressedThisFrame ||
                          g.leftStick.up.wasPressedThisFrame || g.leftStick.down.wasPressedThisFrame))
            return true;

        return false;
    }
}
