using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Porta les regles de la partida: vides, punts, xuixos recollits, victòria i game over.
///
/// NO cal posar-lo a l'escena: es crea sol quan es carrega l'escena "Game"
/// (així no toquem l'escena dels companys). Si algun dia el voleu a l'escena
/// per canviar valors a l'Inspector, afegiu-lo a un objecte i ja no se'n crearà cap altre.
///
/// Final del mapa: poseu un objecte buit amb el tag "Finish" on acaba el mapa.
/// Quan la nau el passa (eix X), la partida s'acaba i es carrega la cinemàtica final.
///
/// PER AL HUD (el fa un altre company): llegiu aquestes dades cada frame
///   GameManager.Instance.Vides, .Punts, .XuixosRecollits, .XuixosTotals, .MultiplicadorPunts
///   GameManager.Instance.Efectes.Actius  -> efectes actius (a.efecte.Nom, a.efecte.Tipus, a.tempsRestant)
/// i subscriviu-vos a l'event MissatgeMostrat per mostrar el text gran de cada efecte:
///   GameManager.Instance.MissatgeMostrat += (text, color) => { ... };
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public const string EscenaJoc = "Game";

    [Header("Escenes")]
    public string escenaVictoria = "CinematicaFinal";
    public string escenaDerrota = "Menu";
    public float segonsAbansDeCanviar = 3f;

    [Header("Regles (GDD)")]
    public int videsInicials = 3;
    public int puntsPerSegon = 1;
    public int puntsPerXuixo = 1000;
    public float segonsInvencibleDespresDeCop = 2f;

    // Estat de la partida (els altres scripts el poden llegir)
    public int Vides { get; private set; }
    public int Punts { get; private set; }
    public int XuixosRecollits { get; private set; }
    public int XuixosTotals { get; private set; }
    public bool PartidaAcabada { get; private set; }

    /// <summary>Multiplicador de punts (Hipervelocitat el posa a 2).</summary>
    public int MultiplicadorPunts { get; set; } = 1;

    /// <summary>S'envia cada cop que s'ha de mostrar un missatge gran (efecte, -1 vida, game over...).</summary>
    public event Action<string, Color> MissatgeMostrat;

    public PlayerScript Jugador { get; private set; }
    public GestorEfectes Efectes { get; private set; }
    public XocsJugador Xocs { get; private set; }

    private Transform meta;
    private float acumuladorSegons;

    // ---------- Creació automàtica a l'escena Game ----------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicialitzar()
    {
        SceneManager.sceneLoaded -= EscenaCarregada;
        SceneManager.sceneLoaded += EscenaCarregada;
        EscenaCarregada(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void EscenaCarregada(Scene escena, LoadSceneMode mode)
    {
        if (escena.name != EscenaJoc || Instance != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    // ---------- Cicle de vida ----------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        Time.timeScale = 1f;
        Vides = videsInicials;

        // La nau del jugador (tag "Player")
        GameObject nau = GameObject.FindGameObjectWithTag("Player");
        if (nau != null)
        {
            Jugador = nau.GetComponent<PlayerScript>();
            Efectes = nau.GetComponent<GestorEfectes>();
            if (Efectes == null) Efectes = nau.AddComponent<GestorEfectes>();
            Xocs = nau.GetComponent<XocsJugador>();
            if (Xocs == null) Xocs = nau.AddComponent<XocsJugador>();
        }
        else
        {
            Debug.LogError("[GameManager] No trobo la nau: ha de tenir el tag 'Player'.");
        }

        // Els xuixos del mapa (tag "Pickup"). Si algun no té l'script Xuixo, l'hi posem.
        GameObject[] xuixos = GameObject.FindGameObjectsWithTag("Pickup");
        XuixosTotals = xuixos.Length > 0 ? xuixos.Length : 10;
        foreach (GameObject x in xuixos)
        {
            if (x.GetComponent<Xuixo>() == null)
            {
                Debug.LogWarning("[GameManager] '" + x.name + "' no té l'script Xuixo. L'hi afegeixo (millor posar-lo al prefab).");
                x.AddComponent<Xuixo>();
            }
        }

        // Final del mapa (tag "Finish")
        GameObject final = GameObject.FindGameObjectWithTag("Finish");
        if (final != null) meta = final.transform;
        else Debug.LogWarning("[GameManager] No hi ha cap objecte amb el tag 'Finish': la partida no s'acabarà en arribar al final del mapa.");
    }

    void Update()
    {
        if (PartidaAcabada) return;

        // 1 punt per segon (GDD)
        acumuladorSegons += Time.deltaTime;
        while (acumuladorSegons >= 1f)
        {
            acumuladorSegons -= 1f;
            SumarPunts(puntsPerSegon);
        }

        // Ha arribat al final del mapa?
        if (meta != null && Jugador != null && Jugador.transform.position.x >= meta.position.x)
            Victoria();
    }

    // ---------- Mètodes que fan servir els altres scripts ----------

    public void SumarPunts(int punts)
    {
        if (PartidaAcabada) return;
        Punts += punts * MultiplicadorPunts;
    }

    public void XuixoRecollit()
    {
        if (PartidaAcabada) return;
        XuixosRecollits++;
        SumarPunts(puntsPerXuixo);
        if (Efectes != null) Efectes.ActivarEfecteAleatori();
    }

    public void SumarVida()
    {
        Vides++;
    }

    /// <summary>La nau ha rebut un cop (meteorit o bala enemiga).</summary>
    public void RebreDany()
    {
        if (PartidaAcabada) return;
        if (Xocs != null && Xocs.EsInvencible) return;

        Vides--;
        if (Vides <= 0)
        {
            Vides = 0;
            GameOver();
        }
        else
        {
            if (Xocs != null) Xocs.IniciarInvencibilitat(segonsInvencibleDespresDeCop);
            MostrarMissatge("-1 VIDA", new Color(1f, 0.35f, 0.35f));
        }
    }

    /// <summary>Per a proves: acaba la partida com si t'haguessis quedat sense vides.</summary>
    public void ForcarGameOver()
    {
        Vides = 0;
        GameOver();
    }

    public void MostrarMissatge(string text, Color color)
    {
        Debug.Log("[Joc] " + text);
        MissatgeMostrat?.Invoke(text, color);
    }

    // ---------- Final de la partida ----------

    void Victoria()
    {
        Acabar(true, "HAS ARRIBAT AL FINAL!", new Color(1f, 0.8f, 0.45f), escenaVictoria);
    }

    void GameOver()
    {
        Acabar(false, "GAME OVER", new Color(1f, 0.35f, 0.35f), escenaDerrota);
    }

    void Acabar(bool guanyat, string missatge, Color color, string escena)
    {
        if (PartidaAcabada) return;

        MostrarMissatge(missatge, color);
        PartidaAcabada = true;

        ResultatsPartida.Guanyat = guanyat;
        ResultatsPartida.Punts = Punts;
        ResultatsPartida.Xuixos = XuixosRecollits;
        ResultatsPartida.XuixosTotals = XuixosTotals;

        // Aturem la nau
        if (Jugador != null)
        {
            Jugador.enabled = false;
            Rigidbody2D rb = Jugador.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (guanyat)
        {
            StartCoroutine(CarregarDespres(escena));
        }
        else
        {
            // Game over: la nau mor i surt la pantalla de GAME OVER, que torna al menú
            if (Xocs != null) Xocs.Morir();
            PantallaGameOver.Mostrar(Punts, XuixosRecollits, XuixosTotals, escena);
        }
    }

    IEnumerator CarregarDespres(string escena)
    {
        yield return new WaitForSecondsRealtime(segonsAbansDeCanviar);

        if (Application.CanStreamedLevelBeLoaded(escena))
            SceneManager.LoadScene(escena);
        else
            Debug.LogError("[GameManager] No es pot carregar l'escena '" + escena + "'. Està a la llista d'escenes del build?");
    }
}
