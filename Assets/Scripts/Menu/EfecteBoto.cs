using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Fa que el botó creixi una mica i canviï el color del text quan està seleccionat
/// (amb el ratolí a sobre, amb el teclat o amb el comandament).
/// </summary>
[RequireComponent(typeof(Selectable))]
public class EfecteBoto : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    public float escalaSeleccionat = 1.08f;
    public float velocitat = 14f;

    [Header("Text del botó (opcional)")]
    public Text etiqueta;
    public Color colorNormal = new Color32(40, 22, 16, 255);
    public Color colorSeleccionat = new Color32(190, 80, 40, 255);

    private Selectable selectable;
    private Vector3 escalaBase;
    private bool seleccionat = false;

    void Awake()
    {
        selectable = GetComponent<Selectable>();
        escalaBase = transform.localScale;
    }

    void OnDisable()
    {
        seleccionat = false;
        transform.localScale = escalaBase;
        if (etiqueta != null) etiqueta.color = colorNormal;
    }

    void Update()
    {
        Vector3 objectiu = seleccionat ? escalaBase * escalaSeleccionat : escalaBase;
        // unscaledDeltaTime: funciona encara que el joc estigui en pausa (timeScale = 0)
        float t = 1f - Mathf.Exp(-velocitat * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, objectiu, t);
    }

    // Quan el ratolí passa per sobre, seleccionem el botó: així ratolí i teclat van sincronitzats
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null && selectable.interactable && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnSelect(BaseEventData eventData)
    {
        seleccionat = true;
        // so de passar per sobre (no en obrir el menú, quan se selecciona JUGAR sol)
        if (Time.timeSinceLevelLoad > 0.3f) Sons.PassarPerSobre();
        if (etiqueta != null) etiqueta.color = colorSeleccionat;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        seleccionat = false;
        if (etiqueta != null) etiqueta.color = colorNormal;
    }
}
