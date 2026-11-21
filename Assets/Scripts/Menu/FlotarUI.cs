using UnityEngine;

/// <summary>
/// Fa "flotar" un element de la UI amunt i avall (i opcionalment el balanceja).
/// Es fa servir pel logo i per la nau del menú.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FlotarUI : MonoBehaviour
{
    [Tooltip("Píxels que puja i baixa")]
    public float amplitud = 12f;
    [Tooltip("Velocitat de l'oscil·lació")]
    public float velocitat = 1.5f;
    [Tooltip("Graus de balanceig (0 = sense rotació)")]
    public float balanceig = 0f;
    [Tooltip("Desfasament perquè dos elements no es moguin igual")]
    public float desfasament = 0f;

    private RectTransform rt;
    private Vector2 posicioBase;
    private float rotacioBase;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        posicioBase = rt.anchoredPosition;
        rotacioBase = rt.localEulerAngles.z;
    }

    void Update()
    {
        float t = Time.unscaledTime * velocitat + desfasament;
        rt.anchoredPosition = posicioBase + Vector2.up * (Mathf.Sin(t) * amplitud);

        if (balanceig != 0f)
            rt.localRotation = Quaternion.Euler(0f, 0f, rotacioBase + Mathf.Sin(t * 0.7f) * balanceig);
    }
}
