using UnityEngine;

/// <summary>
/// "La llaona de Girona" (efecte d'un xuixo): una nau petita que segueix la teva
/// i dispara sola. La crea l'efecte EfecteLlaonaDeGirona i la destrueix quan s'acaba.
/// </summary>
public class NauAjudant : MonoBehaviour
{
    public float cadencia = 0.35f;    // segons entre trets
    public float distancia = 1.4f;    // separació vertical respecte la nau

    private PlayerScript nau;
    private SpriteRenderer srNau, sr;
    private float properDispar;

    public static NauAjudant Crear(PlayerScript nau)
    {
        GameObject go = new GameObject("LaLlaonaDeGirona");
        NauAjudant a = go.AddComponent<NauAjudant>();
        a.nau = nau;
        a.srNau = nau.GetComponentInChildren<SpriteRenderer>();

        a.sr = go.AddComponent<SpriteRenderer>();
        if (a.srNau != null)
        {
            a.sr.sprite = a.srNau.sprite;
            a.sr.sortingLayerID = a.srNau.sortingLayerID;
            a.sr.sortingOrder = a.srNau.sortingOrder;
        }
        a.sr.color = new Color(1f, 0.85f, 0.55f);   // daurada, com una llaona
        go.transform.localScale = nau.transform.lossyScale * 0.55f;
        go.transform.position = nau.transform.position;
        return a;
    }

    void LateUpdate()
    {
        if (nau == null) { Destroy(gameObject); return; }

        // Es posa a sobre de la nau, o a sota si la nau és a la meitat de dalt de la pantalla
        float yCamera = Camera.main != null ? Camera.main.transform.position.y : 0f;
        float costat = nau.transform.position.y > yCamera ? -1f : 1f;
        Vector3 objectiu = nau.transform.position + new Vector3(-0.4f, costat * distancia, 0f);
        transform.position = Vector3.Lerp(transform.position, objectiu, 1f - Mathf.Exp(-10f * Time.deltaTime));

        // Copia l'animació de la nau
        if (srNau != null) { sr.sprite = srNau.sprite; sr.flipX = srNau.flipX; }

        // Dispara sola
        if (Time.time >= properDispar && nau.prefabBala != null)
        {
            properDispar = Time.time + cadencia;
            Quaternion rot = nau.dispararEnrere ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
            GameObject bala = Instantiate(nau.prefabBala, transform.position + rot * Vector3.right * 0.6f, rot);
            bala.transform.localScale *= 0.7f;
            if (nau.balesDePlastic)
            {
                Bala b = bala.GetComponent<Bala>();
                if (b != null) b.FerDePlastic();
            }
        }
    }
}
