using UnityEngine;

/// <summary>
/// CLASSE MARE dels xuixos. Va al prefab "pickup".
///
/// Fa el que tenen en comú tots els xuixos:
///  1. Detectar que la nau (tag "Player") el toca.
///  2. Avisar el GameManager (suma el xuixo, 1000 punts i activa un efecte a l'atzar).
///  3. Fer el so (si n'hi ha) i desaparèixer.
///
/// Si mai voleu un xuixo especial, feu una classe filla i sobreescriviu Recollir():
///
///     public class XuixoDaurat : Xuixo
///     {
///         protected override void Recollir(GameObject nau)
///         {
///             base.Recollir(nau);                       // fa el mateix que un xuixo normal
///             GameManager.Instance.SumarPunts(5000);    // i a més dona punts extra
///         }
///     }
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Xuixo : MonoBehaviour
{
    [Header("So en agafar-lo (opcional)")]
    public AudioClip soRecollir;
    [Range(0f, 1f)] public float volum = 1f;

    private bool recollit = false;

    protected virtual void Awake()
    {
        // El xuixo s'ha de poder travessar: si no, la nau hi xocaria
        foreach (Collider2D c in GetComponents<Collider2D>())
            c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D altre)
    {
        if (recollit) return;                 // evita agafar-lo dues vegades
        if (!altre.CompareTag("Player")) return;

        recollit = true;
        Recollir(altre.gameObject);
    }

    /// <summary>Què passa quan s'agafa. Les classes filles ho poden canviar.</summary>
    protected virtual void Recollir(GameObject nau)
    {
        if (soRecollir != null)
        {
            Vector3 posicio = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(soRecollir, posicio, volum);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.XuixoRecollit();

        Destroy(gameObject);
    }
}
