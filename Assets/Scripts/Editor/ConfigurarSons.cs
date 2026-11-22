using UnityEditor;
using UnityEngine;

/// <summary>
/// Eina d'editor: crea (o actualitza) Assets/Resources/BibliotecaSons.asset
/// amb els sons de la carpeta Assets/Audio.
/// Menú de Unity: AstroXuixos > Configurar sons
/// </summary>
public static class ConfigurarSons
{
    const string RutaAsset = "Assets/Resources/BibliotecaSons.asset";
    const string Carpeta = "Assets/Audio/";

    [MenuItem("AstroXuixos/Configurar sons")]
    public static void Configurar()
    {
        BibliotecaSons b = AssetDatabase.LoadAssetAtPath<BibliotecaSons>(RutaAsset);
        if (b == null)
        {
            b = ScriptableObject.CreateInstance<BibliotecaSons>();
            AssetDatabase.CreateAsset(b, RutaAsset);
        }

        b.musicaMenu = Clip("SoMenuJoc.wav");
        b.clicBoto = Clip("So_Clicar_Buto_Menu.wav");
        b.passarPerSobreBoto = Clip("So_Pasar_Sobre_Boto.wav");
        b.dispar = Clip("dispar_misil_so.wav");
        b.explosioMeteorit = Clip("Explota_Asteroide.wav");
        b.xocNau = Clip("Meteorito_Xoca_Nau.wav");
        b.agafarXuixo = Clip("Xuixo_Pickup.wav");
        b.gameOver = Clip("GameOverSo.wav");

        EditorUtility.SetDirty(b);
        AssetDatabase.SaveAssets();
        Selection.activeObject = b;
        Debug.Log("[Sons] BibliotecaSons configurada a " + RutaAsset);
    }

    static AudioClip Clip(string nom)
    {
        AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Carpeta + nom);
        if (c == null) Debug.LogWarning("[Sons] No trobo " + Carpeta + nom);
        return c;
    }
}
