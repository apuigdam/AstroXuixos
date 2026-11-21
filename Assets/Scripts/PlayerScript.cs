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
        Instantiate(prefabBala, puntoDisparo.position, puntoDisparo.rotation);

        if (dispararTriple)   // efecte Dispar triple: dues bales més en diagonal
        {
            Instantiate(prefabBala, puntoDisparo.position, puntoDisparo.rotation * Quaternion.Euler(0, 0, angleDisparTriple));
            Instantiate(prefabBala, puntoDisparo.position, puntoDisparo.rotation * Quaternion.Euler(0, 0, -angleDisparTriple));
        }
    }


}