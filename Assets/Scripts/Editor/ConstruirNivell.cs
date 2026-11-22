using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Eina d'editor: munta el nivell de l'escena Game segons el GDD.
/// Menú de Unity: AstroXuixos > Construir nivell (Game)
///
///  - Meteorits de 3 mides repartits en 3 trams (inici fàcil, mig, final difícil).
///  - 10 xuixos (prefab pickup): 3 a l'inici, 4 al mig i 3 al final.
///  - Objecte "Final" amb el tag Finish on acaba el mapa (aquí comença la cinemàtica final).
///  - Assigna la bala i el punt de dispar a la nau si no en tenia.
///  - Connecta el HUD (script HUDJoc al Canvas).
///
/// Tot el que crea va dins de l'objecte "Nivell". Es pot tornar a executar:
/// esborra el "Nivell" vell i en fa un de nou (amb la mateixa llavor surt igual).
/// </summary>
public static class ConstruirNivell
{
    const string RutaEscena = "Assets/Scenes/Game.unity";
    const string RutaPickup = "Assets/Prefabs/pickup.prefab";
    const string RutaBala = "Assets/Prefabs/bala.prefab";
    const string RutaSpritesMeteor = "Assets/Sprites/meteoritsgrans.png";
    const int Llavor = 2026;   // canvieu-la per generar un altre nivell

    class Mida
    {
        public string nom; public float escala; public int punts; public int impactes;
        public Mida(string n, float e, int p, int i) { nom = n; escala = e; punts = p; impactes = i; }
        public float Radi => 0.33f * escala;   // radi del collider en unitats del món
    }

    // escala, punts, trets per trencar-lo
    static readonly Mida Petit = new Mida("Petit", 2.4f, 50, 2);
    static readonly Mida Mitja = new Mida("Mitja", 3.5f, 100, 4);
    static readonly Mida Gran = new Mida("Gran", 4.8f, 200, 8);

    class Tram
    {
        public string nom; public float espai; public float pPetit, pMitja; public float pDoble; public int xuixos; public float velocitat;
        public Tram(string n, float e, float pp, float pm, float pd, int x, float v) { nom = n; espai = e; pPetit = pp; pMitja = pm; pDoble = pd; xuixos = x; velocitat = v; }
    }

    // GDD: inici pocs i petits, mig de les 3 mides, final molts mitjans i grans
    static readonly Tram[] Trams =
    {
        //        nom      espai  %petit %mitjà %doble xuixos velocitat
        new Tram("Inici", 5.5f, 0.70f, 0.30f, 0.15f, 3, 0.6f),
        new Tram("Mig",   4.2f, 0.40f, 0.40f, 0.30f, 4, 0.85f),
        new Tram("Final", 3.4f, 0.15f, 0.45f, 0.40f, 3, 1.1f),
    };

    [MenuItem("AstroXuixos/Construir nivell (Game)")]
    public static void Construir()
    {
        Scene escena = SceneManager.GetActiveScene();
        if (escena.path != RutaEscena)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
        }

        GameObject prefabXuixo = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPickup);
        Sprite[] fases = AssetDatabase.LoadAllAssetsAtPath(RutaSpritesMeteor).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (prefabXuixo == null || fases.Length < 3)
        {
            Debug.LogError("[Nivell] Falta el prefab pickup o els 3 sprites de meteoritsgrans.png");
            return;
        }

        // Esborrem el nivell anterior
        foreach (GameObject go in escena.GetRootGameObjects())
            if (go.name == "Nivell") Object.DestroyImmediate(go);

        // ---- Mides del mapa ----
        float xMin = -9f, xMax = 226f;
        GameObject fons = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "backgrounds");
        if (fons != null)
        {
            Renderer[] rs = fons.GetComponentsInChildren<Renderer>();
            if (rs.Length > 0)
            {
                Bounds b = rs[0].bounds;
                foreach (Renderer r in rs) b.Encapsulate(r.bounds);
                xMin = b.min.x; xMax = b.max.x;
            }
        }

        GameObject nau = GameObject.FindGameObjectWithTag("Player");
        float xNau = nau != null ? nau.transform.position.x : xMin + 2f;

        Camera cam = Camera.main;
        float yCentre = cam != null ? cam.transform.position.y : 0f;
        float mitjaAlcada = cam != null ? cam.orthographicSize : 5f;
        float yMin = yCentre - mitjaAlcada, yMax = yCentre + mitjaAlcada;

        // La càmera va 7 unitats per davant de la nau i en veu unes 9 a cada costat:
        // posem el final perquè no es vegi mai més enllà del fons
        float xFinal = xMax - 28f;   // deixem espai perquè la nau surti accelerant en guanyar
        float xInici = xNau + 14f;
        float xFi = xFinal - 6f;
        float llargadaTram = (xFi - xInici) / Trams.Length;

        System.Random rnd = new System.Random(Llavor);

        // ---- Objectes contenidors ----
        GameObject nivell = new GameObject("Nivell");
        Transform pareMeteorits = new GameObject("Meteorits").transform;
        pareMeteorits.SetParent(nivell.transform);
        Transform pareXuixos = new GameObject("Xuixos").transform;
        pareXuixos.SetParent(nivell.transform);

        // ---- Xuixos (primer, perquè els meteorits els respectin) ----
        List<Vector2> posXuixos = new List<Vector2>();
        for (int t = 0; t < Trams.Length; t++)
        {
            float x0 = xInici + t * llargadaTram;
            int n = Trams[t].xuixos;
            for (int i = 0; i < n; i++)
            {
                float x = x0 + llargadaTram * (i + 0.5f) / n + Rang(rnd, -2f, 2f);
                float y = Rang(rnd, yMin + 1.6f, yMax - 1.6f);
                GameObject x_ = (GameObject)PrefabUtility.InstantiatePrefab(prefabXuixo, pareXuixos);
                x_.name = "Xuixo_" + (posXuixos.Count + 1);
                x_.transform.position = new Vector3(x, y, 0f);
                posXuixos.Add(new Vector2(x, y));
            }
        }

        // ---- Meteorits ----
        int numMeteorits = 0;
        for (int t = 0; t < Trams.Length; t++)
        {
            Tram tram = Trams[t];
            float x = xInici + t * llargadaTram;
            float fiTram = x + llargadaTram;
            while (true)
            {
                x += tram.espai * Rang(rnd, 0.8f, 1.2f);
                if (x > fiTram) break;

                Mida m1 = TriarMida(rnd, tram);
                float y1;
                if (!TrobarY(rnd, x, m1, yMin, yMax, posXuixos, null, 0f, out y1)) continue;
                CrearMeteorit(pareMeteorits, fases, m1, x, y1, ++numMeteorits, rnd, tram.velocitat);

                // A vegades un segon meteorit a la mateixa columna, deixant un forat per passar
                if (rnd.NextDouble() < tram.pDoble)
                {
                    Mida m2 = TriarMida(rnd, tram);
                    float y2;
                    if (TrobarY(rnd, x, m2, yMin, yMax, posXuixos, m1, y1, out y2))
                        CrearMeteorit(pareMeteorits, fases, m2, x + Rang(rnd, -0.8f, 0.8f), y2, ++numMeteorits, rnd, tram.velocitat);
                }
            }
        }

        // ---- Final del mapa ----
        GameObject final = new GameObject("Final");
        final.transform.SetParent(nivell.transform);
        final.transform.position = new Vector3(xFinal, yCentre, 0f);
        final.tag = "Finish";

        // ---- Càmera: fons negre (l'espai), per si mai es veu més enllà del fons ----
        if (cam != null)
        {
            Undo.RecordObject(cam, "Fons negre");
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        // ---- Nau: bala i punt de dispar ----
        ConfigurarNau(nau);

        // ---- HUD ----
        ConfigurarHUD(escena);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = nivell;
        Debug.Log("[Nivell] Nivell construït: " + numMeteorits + " meteorits, " + posXuixos.Count +
                  " xuixos i el final a x=" + xFinal.ToString("0.0") + " (mapa de " + xMin.ToString("0") + " a " + xMax.ToString("0") + ").");
    }

    // ================== AJUDES ==================

    static float Rang(System.Random r, float min, float max) => min + (float)r.NextDouble() * (max - min);

    static Mida TriarMida(System.Random r, Tram t)
    {
        double p = r.NextDouble();
        if (p < t.pPetit) return Petit;
        if (p < t.pPetit + t.pMitja) return Mitja;
        return Gran;
    }

    /// Busca una Y on el meteorit no toqui cap xuixo ni l'altre meteorit de la columna
    static bool TrobarY(System.Random rnd, float x, Mida m, float yMin, float yMax,
                        List<Vector2> xuixos, Mida altre, float yAltre, out float y)
    {
        for (int intent = 0; intent < 20; intent++)
        {
            y = Rang(rnd, yMin + m.Radi + 0.3f, yMax - m.Radi - 0.3f);
            bool ok = true;
            foreach (Vector2 xu in xuixos)
                if (Vector2.Distance(xu, new Vector2(x, y)) < m.Radi + 2.2f) { ok = false; break; }
            // deixem un forat de 2,6 unitats perquè la nau hi passi
            if (ok && altre != null && Mathf.Abs(y - yAltre) < m.Radi + altre.Radi + 2.6f) ok = false;
            if (ok) return true;
        }
        y = 0f;
        return false;
    }

    static void CrearMeteorit(Transform pare, Sprite[] fases, Mida m, float x, float y, int num,
                              System.Random rnd, float factorVelocitat)
    {
        GameObject go = new GameObject("Meteorit_" + m.nom + "_" + num);
        go.transform.SetParent(pare);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * m.escala;
        go.transform.rotation = Quaternion.Euler(0f, 0f, (num * 47) % 360);
        go.tag = "Meteor";
        int capa = LayerMask.NameToLayer("Meteors");
        if (capa >= 0) go.layer = capa;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = fases[0];
        sr.sortingOrder = 5;   // per sobre del fons, per sota dels xuixos i de la nau

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.33f;
        col.isTrigger = true;  // la nau el travessa (i perd una vida) en lloc de quedar-s'hi encallada

        Meteor met = go.AddComponent<Meteor>();
        met.fase0 = fases[0];
        met.fase1 = fases[1];
        met.fase2 = fases[2];
        met.punts = m.punts;
        met.impactesPerTrencar = m.impactes;

        // Moviment: els petits es mouen més ràpid que els grans, i al final del nivell tots van més ràpid
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        MovimentMeteorit mov = go.AddComponent<MovimentMeteorit>();
        float vy = m == Petit ? Rang(rnd, 1.6f, 2.4f) : (m == Mitja ? Rang(rnd, 1.0f, 1.7f) : Rang(rnd, 0.5f, 1.0f));
        if (rnd.NextDouble() < 0.5) vy = -vy;
        float vx = -Rang(rnd, 0.2f, 1.0f);
        mov.velocitat = new Vector2(vx, vy) * factorVelocitat;
        mov.gir = Rang(rnd, 10f, 40f) * (rnd.NextDouble() < 0.5 ? -1f : 1f);
    }

    static void ConfigurarNau(GameObject nau)
    {
        if (nau == null) { Debug.LogWarning("[Nivell] No trobo la nau (tag Player)."); return; }
        PlayerScript ps = nau.GetComponent<PlayerScript>();
        if (ps == null) return;

        Undo.RecordObject(ps, "Configurar nau");
        if (ps.prefabBala == null)
            ps.prefabBala = AssetDatabase.LoadAssetAtPath<GameObject>(RutaBala);
        if (ps.puntoDisparo == null)
        {
            Transform punt = nau.GetComponentsInChildren<Transform>()
                .FirstOrDefault(t => t != nau.transform && t.name.ToLower().Contains("punt"));
            ps.puntoDisparo = punt != null ? punt : nau.transform;
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(ps);
    }

    static void ConfigurarHUD(Scene escena)
    {
        Canvas canvas = escena.GetRootGameObjects()
            .Select(g => g.GetComponentInChildren<Canvas>(true)).FirstOrDefault(c => c != null);
        if (canvas == null) { Debug.LogWarning("[Nivell] No trobo el Canvas del HUD."); return; }

        HUDJoc hud = canvas.GetComponent<HUDJoc>();
        if (hud == null) hud = canvas.gameObject.AddComponent<HUDJoc>();

        Transform[] tots = canvas.GetComponentsInChildren<Transform>(true);
        GameObject Buscar(string nom) => tots.FirstOrDefault(t => t.name.ToLower() == nom.ToLower())?.gameObject;

        hud.vida100 = Buscar("Vida100");
        hud.vida66 = Buscar("Vida66");
        hud.vida25 = Buscar("Vida25");
        GameObject punts = Buscar("Puntuacio");
        hud.textPunts = punts != null ? punts.GetComponent<TMP_Text>() : null;

        if (hud.vida100 == null || hud.vida66 == null || hud.vida25 == null || hud.textPunts == null)
            Debug.LogWarning("[Nivell] HUD: no he trobat algun element (Vida100, Vida66, Vida25 o Puntuacio). Assigneu-lo a l'Inspector del HUDJoc.");
    }
}
