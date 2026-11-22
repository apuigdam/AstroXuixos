using UnityEngine;

/// <summary>
/// Una partícula senzilla (espurna, tros de meteorit o flaix): es mou, gira,
/// canvia de mida i es va fent transparent fins que desapareix.
/// La creen els EfectesVisuals; no cal posar-la enlloc.
/// </summary>
public class Particula : MonoBehaviour
{
    public Vector2 velocitat;
    public float gir;
    public float vida = 0.6f;
    public float escalaFinal = 1f;
    public float frenada = 2f;

    private SpriteRenderer sr;
    private float temps;
    private Vector3 escalaInicial;
    private Color colorInicial;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        escalaInicial = transform.localScale;
        colorInicial = sr != null ? sr.color : Color.white;
    }

    void Update()
    {
        temps += Time.deltaTime;
        float p = temps / vida;
        if (p >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(velocitat * Time.deltaTime);
        velocitat *= Mathf.Clamp01(1f - frenada * Time.deltaTime);   // es frena poc a poc
        transform.Rotate(0f, 0f, gir * Time.deltaTime);
        transform.localScale = Vector3.Lerp(escalaInicial, escalaInicial * escalaFinal, p);

        if (sr != null)
        {
            Color c = colorInicial;
            c.a = colorInicial.a * (1f - p);
            sr.color = c;
        }
    }
}
