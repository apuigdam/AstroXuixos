using UnityEngine;

public class Bala : MonoBehaviour
{
    public float velocidad = 10f;

    // Efecte "Bales de plastic": la bala no fa mal als meteorits
    [HideInInspector] public bool deplastic = false;

    private Rigidbody2D rb;
    private Vector2 direccio;

    void Awake()
    {
        // La direccio es fixa en neixer: la bala va SEMPRE recta
        direccio = transform.right;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;          // no pot girar si xoca amb alguna cosa
            rb.linearVelocity = direccio * velocidad;
        }

        // No xoca amb la nau (si no, la desviaria en sortir)
        GameObject nau = GameObject.FindGameObjectWithTag("Player");
        if (nau != null)
        {
            foreach (Collider2D meu in GetComponents<Collider2D>())
                foreach (Collider2D deLaNau in nau.GetComponentsInChildren<Collider2D>())
                    Physics2D.IgnoreCollision(meu, deLaNau);
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = direccio * velocidad;   // sempre la mateixa velocitat i direccio
            rb.angularVelocity = 0f;
        }
    }

    void Update()
    {
        if (rb == null)
            transform.position += (Vector3)(direccio * velocidad * Time.deltaTime);
    }

    public void FerDePlastic()
    {
        deplastic = true;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1f, 0.55f, 0.8f, 0.8f);   // rosa: es veu que es de plastic
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
