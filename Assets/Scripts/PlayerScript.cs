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

    int vides = 1;

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

        playerBody.linearVelocity = new Vector2(forwardSpeed, vertical * verticalSpeed);



        if (gameObject.CompareTag("Meteor"))
        {
            vides--;

        }
    }

    void Disparar()
    {
        Instantiate(prefabBala, puntoDisparo.position, puntoDisparo.rotation);
    }


}