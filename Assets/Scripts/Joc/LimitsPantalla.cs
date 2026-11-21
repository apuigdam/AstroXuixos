using UnityEngine;

/// <summary>
/// Va a la nau (el GameManager l'hi posa sol si no la té).
/// Fa que la nau no pugui sortir per dalt ni per baix de la pantalla.
///
/// Com funciona: abans de cada pas de física mira on acabaria la nau amb la
/// velocitat que porta. Si sortiria de la pantalla, li retalla la velocitat
/// vertical perquè es quedi just a la vora (així no tremola).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class LimitsPantalla : MonoBehaviour
{
    [Tooltip("Espai extra que es deixa a dalt (per exemple, perquè la nau no quedi sota el HUD)")]
    public float margeSuperior = 0f;
    [Tooltip("Espai extra que es deixa a baix")]
    public float margeInferior = 0f;

    private Rigidbody2D rb;
    private Camera cam;
    private float mitjaAlcada;   // la meitat de l'alçada de la nau

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // Mida de la nau, perquè no en surti ni la punta
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) mitjaAlcada = col.bounds.extents.y;
        else
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            mitjaAlcada = sr != null ? sr.bounds.extents.y : 0.5f;
        }
    }

    void FixedUpdate()
    {
        if (cam == null || !cam.orthographic) return;

        float centre = cam.transform.position.y;
        float max = centre + cam.orthographicSize - mitjaAlcada - margeSuperior;
        float min = centre - cam.orthographicSize + mitjaAlcada + margeInferior;

        Vector2 pos = rb.position;
        Vector2 vel = rb.linearVelocity;

        // Si ja és fora (per exemple, al començar), la tornem a dins
        if (pos.y > max) { pos.y = max; rb.position = pos; }
        if (pos.y < min) { pos.y = min; rb.position = pos; }

        // On acabaria després d'aquest pas de física?
        float seguentY = pos.y + vel.y * Time.fixedDeltaTime;
        if (seguentY > max) vel.y = (max - pos.y) / Time.fixedDeltaTime;
        if (seguentY < min) vel.y = (min - pos.y) / Time.fixedDeltaTime;

        rb.linearVelocity = vel;
    }
}
