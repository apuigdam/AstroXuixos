using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] float forwardSpeed = 2.0f;
    [SerializeField] float verticalSpeed = 5.0f;

    Rigidbody2D playerBody;


    public GameObject prefabBala;
    public Transform puntoDisparo;

    public float tiempoEntreDisparos = 0.5f; // Retardo en segundos
    private float proximoDisparo = 0f;

    // --- Efectes dels xuixos (els canvia el GestorEfectes, no cal tocar-ho a l'Inspector) ---
    // Les vides ara les porta el GameManager.
    [HideInInspector] public bool controlsInvertits = false;
    [HideInInspector] public float multiplicadorVelocitat = 1f;
    [HideInInspector] public bool dispararTriple = false;
    [HideInInspector] public bool dispararEnrere = false;   // efecte Mode mirall
    [HideInInspector] public bool balesDePlastic = false;   // efecte Bales de plastic
    public float angleDisparTriple = 12f;

    void Start()
    {
        playerBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= proximoDisparo)
        {
            proximoDisparo = Time.time + tiempoEntreDisparos;
            Disparar();
        }
        float vertical = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            vertical = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            vertical = -1f;
        }

        if (controlsInvertits)
        {
            vertical = -vertical;   // efecte Tecles invertides
        }

        playerBody.linearVelocity = new Vector2(forwardSpeed * multiplicadorVelocitat, vertical * verticalSpeed);
    }

    void Disparar()
    {
        if (prefabBala == null || puntoDisparo == null) return;
        Sons.Dispar();

        GameObject central = CrearBala(0f);

        if (dispararTriple)   // efecte Dispar triple: dues bales més en diagonal
        {
            GameObject dalt = CrearBala(angleDisparTriple);
            GameObject baix = CrearBala(-angleDisparTriple);
            // que les tres bales no xoquin entre elles en sortir del mateix punt
            IgnorarXocs(central, dalt);
            IgnorarXocs(central, baix);
            IgnorarXocs(dalt, baix);
        }
    }

    GameObject CrearBala(float angle)
    {
        if (dispararEnrere) angle += 180f;   // efecte Mode mirall: dispara cap enrere
        GameObject bala = Instantiate(prefabBala, puntoDisparo.position, puntoDisparo.rotation * Quaternion.Euler(0, 0, angle));
        if (balesDePlastic)
        {
            Bala b = bala.GetComponent<Bala>();
            if (b != null) b.FerDePlastic();
        }
        return bala;
    }

    static void IgnorarXocs(GameObject a, GameObject b)
    {
        Collider2D ca = a.GetComponent<Collider2D>();
        Collider2D cb = b.GetComponent<Collider2D>();
        if (ca != null && cb != null) Physics2D.IgnoreCollision(ca, cb);
    }


}