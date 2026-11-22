using UnityEngine;

/// <summary>
/// Fa moure un meteorit: va cap a la nau (cap a l'esquerra), rebota amunt i avall
/// contra les vores de la pantalla i gira sobre si mateix.
/// Només es comença a moure quan és a prop de la càmera, perquè el nivell
/// arribi a la pantalla tal com s'ha dissenyat.
/// El posa l'eina AstroXuixos > Construir nivell (Game) amb valors segons la mida i el tram.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovimentMeteorit : MonoBehaviour
{
    [Tooltip("Unitats per segon. X negativa = cap a la nau")]
    public Vector2 velocitat = new Vector2(-0.6f, 1f);
    [Tooltip("Graus per segon")]
    public float gir = 20f;
    [Tooltip("Distància a la càmera (eix X) a partir de la qual es comença a moure")]
    public float distanciaActivacio = 20f;

    private Rigidbody2D rb;
    private Camera cam;
    private float radi;
    private bool actiu = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;   // el movem nosaltres, la física no l'empeny
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        cam = Camera.main;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        float escala = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        radi = col != null ? col.radius * escala : 0.5f;
    }

    void FixedUpdate()
    {
        if (cam == null) return;

        if (!actiu)
        {
            if (rb.position.x - cam.transform.position.x > distanciaActivacio) return;
            actiu = true;
        }

        Vector2 seguent = rb.position + velocitat * Time.fixedDeltaTime;

        // Rebota contra la part de dalt i de baix de la pantalla (pot sortir-ne una mica)
        float centre = cam.transform.position.y;
        float max = centre + cam.orthographicSize - radi * 0.6f;
        float min = centre - cam.orthographicSize + radi * 0.6f;
        if (seguent.y > max && velocitat.y > 0f) velocitat.y = -velocitat.y;
        if (seguent.y < min && velocitat.y < 0f) velocitat.y = -velocitat.y;

        rb.MovePosition(seguent);
        rb.MoveRotation(rb.rotation + gir * Time.fixedDeltaTime);
    }
}
