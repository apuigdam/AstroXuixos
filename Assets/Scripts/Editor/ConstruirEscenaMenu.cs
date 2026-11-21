using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Eina d'editor: munta tota l'escena Menu automàticament.
/// Menú de Unity: AstroXuixos > Construir escena Menu
/// Es pot tornar a executar sempre que vulguis: esborra el menú vell i el torna a crear.
/// </summary>
public static class ConstruirEscenaMenu
{
    const string RutaEscena = "Assets/Scenes/Menu.unity";
    const string CarpetaSprites = "Assets/Sprites/Menu/";
    const string RutaFont = "Assets/Fonts/PressStart2P-Regular.ttf";

    static readonly Color ColorTextBoto = new Color32(40, 22, 16, 255);
    static readonly Color ColorTaronja = new Color32(255, 200, 120, 255);
    static readonly Color ColorCrema = new Color32(246, 236, 214, 255);

    static Font font;

    [MenuItem("AstroXuixos/Construir escena Menu")]
    public static void Construir()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // 1) Configurar les imatges com a sprites pixel art (sense suavitzat)
        ConfigurarSprite("Fons_Catedral.png", Vector4.zero);
        ConfigurarSprite("Logo_AstroXuixos.png", Vector4.zero);
        ConfigurarSprite("Nau_Menu.png", Vector4.zero);
        ConfigurarSprite("Boto_Pixel.png", new Vector4(24, 24, 24, 24));   // 9-slice
        ConfigurarSprite("Panell_Pixel.png", new Vector4(24, 24, 24, 24)); // 9-slice
        font = CarregarFont();

        // 2) Obrir (o crear) l'escena Menu
        Scene escena;
        if (File.Exists(RutaEscena))
            escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
        else
        {
            escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(escena, RutaEscena);
        }

        // Esborrem el que hàgim creat en una execució anterior
        foreach (GameObject go in escena.GetRootGameObjects())
        {
            if (go.name == "Canvas_Menu" || go.name == "EventSystem" || go.name == "MenuPrincipal")
                Object.DestroyImmediate(go);
        }

        // 3) Càmera
        Camera cam = escena.GetRootGameObjects()
            .Select(g => g.GetComponentInChildren<Camera>())
            .FirstOrDefault(c => c != null);
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(14, 10, 32, 255);
        }

        // 4) EventSystem (amb el mòdul del nou Input System)
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        // 5) Canvas
        GameObject canvasGO = new GameObject("Canvas_Menu", typeof(RectTransform), typeof(Canvas),
                                             typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Transform arrel = canvasGO.transform;

        // --- Fons: la catedral de Girona, enfosquida perquè destaquin els botons ---
        Sprite spriteFons = CarregarSprite("Fons_Catedral.png");
        RectTransform fons = CrearUI("Fons", arrel);
        Image imgFons = fons.gameObject.AddComponent<Image>();
        imgFons.sprite = spriteFons;
        imgFons.color = new Color(0.55f, 0.55f, 0.62f, 1f);
        imgFons.raycastTarget = false;
        Estirar(fons);
        AspectRatioFitter fitter = fons.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = spriteFons != null ? spriteFons.rect.width / spriteFons.rect.height : 1f;

        RectTransform vel = CrearUI("Vel", arrel);
        Image imgVel = vel.gameObject.AddComponent<Image>();
        imgVel.color = new Color(0.04f, 0.03f, 0.12f, 0.35f);
        imgVel.raycastTarget = false;
        Estirar(vel);

        // --- Nau decorativa a la dreta ---
        RectTransform nau = CrearImatge("Nau", arrel, CarregarSprite("Nau_Menu.png"));
        nau.anchoredPosition = new Vector2(640, -150);
        nau.sizeDelta = new Vector2(351, 377);
        nau.localEulerAngles = new Vector3(0, 0, -12);
        FlotarUI flotarNau = nau.gameObject.AddComponent<FlotarUI>();
        flotarNau.amplitud = 18f;
        flotarNau.velocitat = 1.1f;
        flotarNau.balanceig = 4f;
        flotarNau.desfasament = 1.3f;

        // --- Logo ---
        RectTransform logo = CrearImatge("Logo", arrel, CarregarSprite("Logo_AstroXuixos.png"));
        logo.anchorMin = logo.anchorMax = new Vector2(0.5f, 1f);
        logo.pivot = new Vector2(0.5f, 1f);
        logo.anchoredPosition = new Vector2(0, -70);
        logo.sizeDelta = new Vector2(1000, 412);
        FlotarUI flotarLogo = logo.gameObject.AddComponent<FlotarUI>();
        flotarLogo.amplitud = 10f;
        flotarLogo.velocitat = 1.6f;

        // --- Botons ---
        RectTransform botons = CrearUI("Botons", arrel);
        botons.anchoredPosition = new Vector2(0, -230);
        botons.sizeDelta = new Vector2(600, 400);
        VerticalLayoutGroup layout = botons.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Button botoJugar = CrearBoto("BotoJugar", "JUGAR", botons);
        Button botoControls = CrearBoto("BotoControls", "CONTROLS", botons);
        Button botoSortir = CrearBoto("BotoSortir", "SORTIR", botons);

        // --- Peu de pàgina ---
        RectTransform peu = CrearText("Peu", arrel, "GameJam La Girotrònica 2026", 18,
                                      new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleCenter);
        peu.anchorMin = peu.anchorMax = new Vector2(0.5f, 0f);
        peu.pivot = new Vector2(0.5f, 0f);
        peu.anchoredPosition = new Vector2(0, 24);
        peu.sizeDelta = new Vector2(1200, 30);

        // --- Panell de controls (amagat al principi) ---
        RectTransform panell = CrearUI("PanellControls", arrel);
        Estirar(panell);
        Image imgPanell = panell.gameObject.AddComponent<Image>();
        imgPanell.color = new Color(0f, 0f, 0f, 0.75f); // bloqueja els clics als botons de darrere

        RectTransform caixa = CrearUI("Caixa", panell);
        caixa.sizeDelta = new Vector2(1040, 660);
        Image imgCaixa = caixa.gameObject.AddComponent<Image>();
        imgCaixa.sprite = CarregarSprite("Panell_Pixel.png");
        imgCaixa.type = Image.Type.Sliced;

        RectTransform titol = CrearText("Titol", caixa, "CONTROLS", 48, ColorTaronja, TextAnchor.MiddleCenter);
        titol.anchorMin = titol.anchorMax = new Vector2(0.5f, 1f);
        titol.pivot = new Vector2(0.5f, 1f);
        titol.anchoredPosition = new Vector2(0, -60);
        titol.sizeDelta = new Vector2(900, 60);

        // Dues columnes: acció (esquerra) i tecla (dreta). Edita els textos des de l'Inspector.
        string accions = "Moure la nau\n\nDisparar\n\nPausa";
        string tecles = "Fletxes amunt/avall\n\nEspai\n\nEsc";
        RectTransform colAccions = CrearText("Accions", caixa, accions, 26, ColorCrema, TextAnchor.UpperLeft);
        colAccions.anchoredPosition = new Vector2(-200, 30);
        colAccions.sizeDelta = new Vector2(440, 260);
        RectTransform colTecles = CrearText("Tecles", caixa, tecles, 26, ColorTaronja, TextAnchor.UpperRight);
        colTecles.anchoredPosition = new Vector2(220, 30);
        colTecles.sizeDelta = new Vector2(440, 260);

        RectTransform consell = CrearText("Consell", caixa,
            "Recupera els xuixos robats:\ncada un canvia les regles!", 20,
            new Color(1f, 1f, 1f, 0.75f), TextAnchor.MiddleCenter);
        consell.anchoredPosition = new Vector2(0, -125);
        consell.sizeDelta = new Vector2(900, 70);

        Button botoTornar = CrearBoto("BotoTornar", "TORNAR", caixa);
        RectTransform rtTornar = (RectTransform)botoTornar.transform;
        rtTornar.anchorMin = rtTornar.anchorMax = new Vector2(0.5f, 0f);
        rtTornar.pivot = new Vector2(0.5f, 0f);
        rtTornar.anchoredPosition = new Vector2(0, 50);
        rtTornar.sizeDelta = new Vector2(420, 100);
        // Que el teclat no salti als botons del menu de darrere
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        botoTornar.navigation = nav;

        panell.gameObject.SetActive(false);

        // 6) Script del menú i connexió dels botons
        GameObject menuGO = new GameObject("MenuPrincipal");
        MenuPrincipal menu = menuGO.AddComponent<MenuPrincipal>();
        menu.escenaJugar = "CinematicaInicial";
        menu.panellControls = panell.gameObject;
        menu.botoJugar = botoJugar.gameObject;
        menu.botoControls = botoControls.gameObject;
        menu.botoTornar = botoTornar.gameObject;

        UnityEventTools.AddPersistentListener(botoJugar.onClick, menu.Jugar);
        UnityEventTools.AddPersistentListener(botoControls.onClick, menu.ObrirControls);
        UnityEventTools.AddPersistentListener(botoSortir.onClick, menu.Sortir);
        UnityEventTools.AddPersistentListener(botoTornar.onClick, menu.TancarControls);

        // 7) Guardar l'escena i posar-la la primera a la llista d'escenes del build
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        ActualitzarLlistaEscenes();

        Selection.activeGameObject = menuGO;
        Debug.Log("[Menu] Escena Menu construïda correctament!");
    }

    // ================== AJUDES ==================

    static void ConfigurarSprite(string nom, Vector4 vora)
    {
        string ruta = CarpetaSprites + nom;
        TextureImporter ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti == null)
        {
            Debug.LogWarning("[Menu] No trobo la imatge " + ruta);
            return;
        }
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.filterMode = FilterMode.Point;          // pixel art nítid
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.maxTextureSize = 4096;
        ti.spriteBorder = vora;
        ti.SaveAndReimport();
    }

    static Sprite CarregarSprite(string nom)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(CarpetaSprites + nom);
        if (s == null) Debug.LogWarning("[Menu] No s'ha pogut carregar el sprite " + nom);
        return s;
    }

    static Font CarregarFont()
    {
        TrueTypeFontImporter fi = AssetImporter.GetAtPath(RutaFont) as TrueTypeFontImporter;
        if (fi != null && fi.fontRenderingMode != FontRenderingMode.HintedRaster)
        {
            fi.fontRenderingMode = FontRenderingMode.HintedRaster; // lletres pixelades nítides
            fi.SaveAndReimport();
        }
        Font f = AssetDatabase.LoadAssetAtPath<Font>(RutaFont);
        if (f == null)
        {
            Debug.LogWarning("[Menu] No trobo la font pixel, faig servir la de Unity.");
            f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        return f;
    }

    static RectTransform CrearUI(string nom, Transform pare)
    {
        GameObject go = new GameObject(nom, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(pare, false);
        return rt;
    }

    static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static RectTransform CrearImatge(string nom, Transform pare, Sprite sprite)
    {
        RectTransform rt = CrearUI(nom, pare);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return rt;
    }

    static RectTransform CrearText(string nom, Transform pare, string contingut, int mida, Color color, TextAnchor alineacio)
    {
        RectTransform rt = CrearUI(nom, pare);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = contingut;
        t.fontSize = mida;
        t.color = color;
        t.alignment = alineacio;
        t.lineSpacing = 1.1f;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return rt;
    }

    static Button CrearBoto(string nom, string text, Transform pare)
    {
        RectTransform rt = CrearUI(nom, pare);
        rt.sizeDelta = new Vector2(520, 110);

        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = CarregarSprite("Boto_Pixel.png");
        img.type = Image.Type.Sliced;

        Button boto = rt.gameObject.AddComponent<Button>();
        boto.targetGraphic = img;
        ColorBlock colors = boto.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.88f, 0.65f);
        colors.selectedColor = new Color(1f, 0.88f, 0.65f);
        colors.pressedColor = new Color(0.9f, 0.62f, 0.4f);
        colors.fadeDuration = 0.08f;
        boto.colors = colors;

        LayoutElement le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 520;
        le.preferredHeight = 110;

        RectTransform rtText = CrearText("Text", rt, text, 40, ColorTextBoto, TextAnchor.MiddleCenter);
        Estirar(rtText);
        rtText.offsetMin = new Vector2(0, 12); // l'ombra de sota del botó ocupa uns píxels

        EfecteBoto efecte = rt.gameObject.AddComponent<EfecteBoto>();
        efecte.etiqueta = rtText.GetComponent<Text>();
        efecte.colorNormal = ColorTextBoto;

        return boto;
    }

    static void ActualitzarLlistaEscenes()
    {
        string[] ordre =
        {
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/CinematicaInicial.unity",
            "Assets/Scenes/Game.unity",
            "Assets/Scenes/CinematicaFinal.unity",
        };

        List<EditorBuildSettingsScene> llista = new List<EditorBuildSettingsScene>();
        foreach (string ruta in ordre)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ruta) != null)
                llista.Add(new EditorBuildSettingsScene(ruta, true));
        }
        // Mantenim les altres escenes que ja hi hagués, al final
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            if (!llista.Any(x => x.path == s.path))
                llista.Add(s);
        }
        EditorBuildSettings.scenes = llista.ToArray();
    }
}
