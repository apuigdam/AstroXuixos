using UnityEngine;

/// <summary>
/// Efectes visuals fets per codi (no cal cap prefab ni material):
///   EfectesVisuals.Explosio(posició, mida, sprite del tros);
/// Crea un flaix taronja, espurnes i trossos que surten disparats.
/// </summary>
public static class EfectesVisuals
{
    private static Sprite cercle;
    private static Sprite quadrat;

    /// <summary>Cercle blanc difuminat (per al flaix). Es crea una sola vegada.</summary>
    static Sprite Cercle()
    {
        if (cercle != null) return cercle;
        const int n = 64;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                float a = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        }
        tex.Apply();
        cercle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);   // mida 1x1 unitats
        return cercle;
    }

    /// <summary>Quadradet blanc (per a les espurnes, estil pixel art).</summary>
    static Sprite Quadrat()
    {
        if (quadrat != null) return quadrat;
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] blanc = new Color[16];
        for (int i = 0; i < 16; i++) blanc[i] = Color.white;
        tex.SetPixels(blanc);
        tex.Apply();
        quadrat = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);    // mida 1x1 unitats
        return quadrat;
    }

    /// <summary>Explosió d'un meteorit. "mida" és el diàmetre aproximat en unitats.</summary>
    public static void Explosio(Vector3 posicio, float mida, Sprite tros)
    {
        // Flaix
        Crear("Flaix", Cercle(), posicio, mida * 1.4f, new Color(1f, 0.75f, 0.35f, 0.9f),
              Vector2.zero, 0f, 0.3f, 1.8f, 12);

        // Espurnes
        int espurnes = 10 + Mathf.RoundToInt(mida * 2f);
        for (int i = 0; i < espurnes; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Color c = Color.Lerp(new Color(1f, 0.95f, 0.5f), new Color(1f, 0.45f, 0.15f), Random.value);
            Crear("Espurna", Quadrat(), posicio, Random.Range(0.1f, 0.2f), c,
                  dir * Random.Range(3f, 7f) * (0.6f + mida * 0.15f), Random.Range(-360f, 360f),
                  Random.Range(0.35f, 0.7f), 0.3f, 13);
        }

        // Trossos del meteorit
        if (tros != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                Crear("Tros", tros, posicio, mida * 0.3f, Color.white,
                      dir * Random.Range(1.5f, 4f), Random.Range(-400f, 400f),
                      Random.Range(0.7f, 1f), 0.4f, 11);
            }
        }
    }

    static void Crear(string nom, Sprite sprite, Vector3 posicio, float escala, Color color,
                      Vector2 velocitat, float gir, float vida, float escalaFinal, int ordre)
    {
        GameObject go = new GameObject(nom);
        go.transform.position = posicio;
        go.transform.localScale = Vector3.one * escala;
        go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = ordre;

        Particula p = go.AddComponent<Particula>();
        p.velocitat = velocitat;
        p.gir = gir;
        p.vida = vida;
        p.escalaFinal = escalaFinal;
    }
}
