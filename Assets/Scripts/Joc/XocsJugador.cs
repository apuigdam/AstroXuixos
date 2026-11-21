using System.Collections;
using UnityEngine;

/// <summary>
/// Va a la nau (el GameManager l'hi posa sol).
/// - Si xoca amb un meteorit (tag "Meteor") perd una vida.
/// - Després d'un cop, 2 segons d'invencibilitat fent pampallugues (GDD).
/// - Amb l'efecte Trenca-asteroides, els meteorits es trenquen en xocar-hi i no fan mal.
/// - Morir(): animació de mort quan s'acaben les vides (gira, s'encongeix i desapareix).
/// </summary>
public class XocsJugador : MonoBehaviour
{
    [HideInInspector] public bool trencaAsteroides = false;

    public Color colorTrencaAsteroides = new Color(1f, 0.6f, 0.3f);

    public float duradaMort = 0.9f;

    private float invencibleFins = 0f;
    private SpriteRenderer[] sprites;
    private bool mort = false;

    public bool EsInvencible => trencaAsteroides || Time.time < invencibleFins;

    void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    public void IniciarInvencibilitat(float segons)
    {
        invencibleFins = Time.time + segons;
    }

    public void Morir()
    {
        if (mort) return;
        mort = true;
        StartCoroutine(AnimacioMort());
    }

    IEnumerator AnimacioMort()
    {
        // La nau ja no xoca amb res i la física no la mou
        foreach (Collider2D c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Vector3 escalaInicial = transform.localScale;
        float t = 0f;
        while (t < duradaMort)
        {
            t += Time.deltaTime;
            float p = t / duradaMort;   // de 0 a 1

            transform.Rotate(0f, 0f, 720f * Time.deltaTime);              // gira
            transform.localScale = escalaInicial * (1f - 0.7f * p);      // s'encongeix
            foreach (SpriteRenderer sr in sprites)
            {
                if (sr == null) continue;
                sr.enabled = true;
                sr.color = Color.Lerp(new Color(1f, 0.4f, 0.3f, 1f), new Color(1f, 0.4f, 0.3f, 0f), p); // vermella i es fon
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in sprites)
            if (sr != null) sr.enabled = false;
    }

    void OnCollisionEnter2D(Collision2D xoc) { Xocar(xoc.gameObject); }
    void OnTriggerEnter2D(Collider2D altre) { Xocar(altre.gameObject); }

    void Xocar(GameObject altre)
    {
        if (!altre.CompareTag("Meteor")) return;

        if (trencaAsteroides)
        {
            Meteor meteor = altre.GetComponentInParent<Meteor>();
            if (meteor != null) meteor.Destruir();
            else Destroy(altre);
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.RebreDany();
    }

    void Update()
    {
        if (mort) return;

        bool pampallugues = Time.time < invencibleFins;
        bool visible = !pampallugues || Mathf.Repeat(Time.time * 10f, 1f) < 0.5f;

        foreach (SpriteRenderer sr in sprites)
        {
            if (sr == null) continue;
            sr.enabled = visible;
            sr.color = trencaAsteroides ? colorTrencaAsteroides : Color.white;
        }
    }
}
