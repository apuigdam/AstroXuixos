using UnityEngine;

public class Meteor : MonoBehaviour
{
    public Sprite fase0; // Sprite inicial
    public Sprite fase1; // Sprite tras 1er impacto
    public Sprite fase2; // Sprite tras 2do impacto
    public GameObject explosionPrefab;
    [Tooltip("Punts que dona en destruir-lo (GDD: petit 50, mitja 100, gran 200)")]
    public int punts = 50;

    private int hits = 0;
    private bool destruit = false;
    private SpriteRenderer sr;
    private Animator anim;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Si había un Animator controlando el sprite, lo apagamos para que no pise las fases
        if (anim != null)
        {
            anim.enabled = false;
        }

        if (sr != null && fase0 != null)
        {
            sr.sprite = fase0;
        }
    }

    // Funciona si el Collider2D del meteorito o la bala tiene "Is Trigger" marcado
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            RecibirImpacto();
            Destroy(other.gameObject);
        }
    }

    // Funciona si chocan con colisión física normal (sin Is Trigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            RecibirImpacto();
            Destroy(collision.gameObject);
        }
    }
        
    public void RecibirImpacto()
    {
        hits++;

        if (hits == 1)
        {
            if (sr != null && fase1 != null) sr.sprite = fase1;
        }
        else if (hits == 2)
        {
            if (sr != null && fase2 != null) sr.sprite = fase2;
        }
        else if (hits >= 3)
        {
            Destruir();
        }
    }

    // Explota, dona els punts i desapareix (tambe el fa servir l'efecte Trenca-asteroides)
    public void Destruir()
    {
        if (destruit) return;
        destruit = true;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SumarPunts(punts);
        }
        Destroy(gameObject);
    }
}