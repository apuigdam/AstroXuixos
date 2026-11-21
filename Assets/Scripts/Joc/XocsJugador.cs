using UnityEngine;

/// <summary>
/// Va a la nau (el GameManager l'hi posa sol).
/// - Si xoca amb un meteorit (tag "Meteor") perd una vida.
/// - Després d'un cop, 2 segons d'invencibilitat fent pampallugues (GDD).
/// - Amb l'efecte Trenca-asteroides, els meteorits es trenquen en xocar-hi i no fan mal.
/// </summary>
public class XocsJugador : MonoBehaviour
{
    [HideInInspector] public bool trencaAsteroides = false;

    public Color colorTrencaAsteroides = new Color(1f, 0.6f, 0.3f);

    private float invencibleFins = 0f;
    private SpriteRenderer[] sprites;

    public bool EsInvencible => trencaAsteroides || Time.time < invencibleFins;

    void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    public void IniciarInvencibilitat(float segons)
    {
        invencibleFins = Time.time + segons;
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
