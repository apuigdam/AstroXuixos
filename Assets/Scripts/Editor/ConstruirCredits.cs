using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Eina d'editor: afegeix els crèdits a l'escena CinematicaFinal.
/// Menú de Unity: AstroXuixos > Construir crèdits (CinematicaFinal)
/// Es pot tornar a executar: esborra els crèdits vells i els torna a crear.
/// </summary>
public static class ConstruirCredits
{
    const string RutaEscena = "Assets/Scenes/CinematicaFinal.unity";
    const string RutaFont = "Assets/Fonts/PressStart2P-Regular.ttf";

    static readonly Color Taronja = new Color32(255, 200, 120, 255);
    static readonly Color Crema = new Color32(246, 236, 214, 255);
    static readonly Color Gris = new Color(1f, 1f, 1f, 0.6f);

    enum Tipus { Titol, Seccio, Nom, Text }

    // Crèdits EN ORDRE DE LECTURA, agrupats: cada grup és un títol + els seus noms.
    // Després es poden editar a la Hierarchy (Canvas_Credits > Contingut > Grup_...).
    static readonly (Tipus tipus, string text)[][] Grups =
    {
        new[] { (Tipus.Titol, "ASTROXUIXOS"), (Tipus.Text, "GameJam La Girotrònica 2026") },
        new[]
        {
            (Tipus.Seccio, "PLATEA FC"),
            (Tipus.Nom, "Martí Colomer"),
            (Tipus.Nom, "Arnau Puig"),
            (Tipus.Nom, "Pol Piris"),
            (Tipus.Nom, "Lluc Bassó"),
            (Tipus.Nom, "Quim Garijo"),
            (Tipus.Nom, "Varinder Singh"),
            (Tipus.Nom, "Najib El Qasmy"),
        },
        new[] { (Tipus.Seccio, "GRÀCIES PER JUGAR!") },
    };

    [MenuItem("AstroXuixos/Construir crèdits (CinematicaFinal)")]
    public static void Construir()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

        // Esborrem el que hàgim creat abans
        foreach (GameObject go in escena.GetRootGameObjects())
            if (go.name == "Canvas_Credits")
                Object.DestroyImmediate(go);

        // Càmera amb fons negre (perquè quan el vídeo s'atura no es vegi el cel blau)
        Camera cam = escena.GetRootGameObjects()
            .Select(g => g.GetComponentInChildren<Camera>()).FirstOrDefault(c => c != null);
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        // El Video Player: li traiem el ControladorCinematica si el té,
        // perquè aquí qui decideix quan canviar d'escena són els crèdits
        VideoPlayer video = escena.GetRootGameObjects()
            .Select(g => g.GetComponentInChildren<VideoPlayer>()).FirstOrDefault(v => v != null);
        if (video != null)
        {
            ControladorCinematica cc = video.GetComponent<ControladorCinematica>();
            if (cc != null) Object.DestroyImmediate(cc);
            video.isLooping = false;
        }
        else
        {
            Debug.LogWarning("[Credits] No he trobat cap Video Player a l'escena.");
        }

        Font font = AssetDatabase.LoadAssetAtPath<Font>(RutaFont);
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Canvas
        GameObject canvasGO = new GameObject("Canvas_Credits", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Contingut: columna de textos que es mou
        GameObject contGO = new GameObject("Contingut", typeof(RectTransform));
        contGO.layer = canvasGO.layer;
        RectTransform cont = (RectTransform)contGO.transform;
        cont.SetParent(canvasGO.transform, false);
        cont.sizeDelta = new Vector2(1600, 0);
        VerticalLayoutGroup layout = contGO.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 110;   // espai entre grups
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = contGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (var grup in Grups)
        {
            // Cada grup té el seu propi layout perquè el títol quedi sempre a sobre dels noms
            GameObject grupGO = new GameObject("Grup_" + grup[0].text, typeof(RectTransform));
            grupGO.layer = canvasGO.layer;
            grupGO.transform.SetParent(cont, false);
            VerticalLayoutGroup lg = grupGO.AddComponent<VerticalLayoutGroup>();
            lg.spacing = 22;
            lg.childAlignment = TextAnchor.UpperCenter;
            lg.childControlWidth = true;
            lg.childControlHeight = true;
            lg.childForceExpandWidth = true;
            lg.childForceExpandHeight = false;

            foreach (var linia in grup)
            {
                GameObject go = new GameObject(linia.tipus + "_" + linia.text, typeof(RectTransform), typeof(Text));
                go.layer = canvasGO.layer;
                go.transform.SetParent(grupGO.transform, false);
                Text t = go.GetComponent<Text>();
                t.font = font;
                t.text = linia.text;
                t.alignment = TextAnchor.MiddleCenter;
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                t.raycastTarget = false;

                switch (linia.tipus)
                {
                    case Tipus.Titol:  t.fontSize = 64; t.color = Taronja; break;
                    case Tipus.Seccio: t.fontSize = 32; t.color = Taronja; break;
                    case Tipus.Nom:    t.fontSize = 40; t.color = Crema;   break;
                    default:           t.fontSize = 24; t.color = Gris;    break;
                }
            }
        }

        // Script que fa moure els crèdits
        CreditsFinals credits = canvasGO.AddComponent<CreditsFinals>();
        credits.video = video;
        credits.contingut = cont;
        credits.direccio = CreditsFinals.Direccio.DaltABaix;
        credits.invertirOrdre = false;   // el bloc baixa tal com es llegeix
        credits.segonInici = 22.5f;
        credits.escenaFinal = "Menu";

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = canvasGO;
        Debug.Log("[Credits] Crèdits afegits a CinematicaFinal!");
    }
}
