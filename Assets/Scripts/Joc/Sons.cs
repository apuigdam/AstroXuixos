using UnityEngine;

/// <summary>
/// Fa sonar els efectes de so des de qualsevol script, sense haver de posar res a l'escena.
/// Ús:  Sons.Dispar();   Sons.Reproduir(unClip, 0.8f);
/// Els sons surten de BibliotecaSons (Assets/Resources/BibliotecaSons.asset).
/// </summary>
public static class Sons
{
    private static BibliotecaSons biblioteca;
    private static AudioSource font;

    public static BibliotecaSons Biblioteca
    {
        get
        {
            if (biblioteca == null) biblioteca = Resources.Load<BibliotecaSons>("BibliotecaSons");
            return biblioteca;
        }
    }

    /// <summary>Fa sonar un so una vegada (en 2D, sempre al mateix volum).</summary>
    public static void Reproduir(AudioClip clip, float volum = 1f)
    {
        if (clip == null) return;
        if (font == null)
        {
            GameObject go = new GameObject("Sons");
            Object.DontDestroyOnLoad(go);
            font = go.AddComponent<AudioSource>();
            font.playOnAwake = false;
            font.spatialBlend = 0f;
        }
        font.PlayOneShot(clip, volum);
    }

    // Dreceres per als sons del joc
    public static void ClicBoto()        { if (Biblioteca != null) Reproduir(Biblioteca.clicBoto, 0.8f); }
    public static void PassarPerSobre()  { if (Biblioteca != null) Reproduir(Biblioteca.passarPerSobreBoto, 0.5f); }
    public static void Dispar()          { if (Biblioteca != null) Reproduir(Biblioteca.dispar, 0.35f); }
    public static void Explosio()        { if (Biblioteca != null) Reproduir(Biblioteca.explosioMeteorit, 0.7f); }
    public static void XocNau()          { if (Biblioteca != null) Reproduir(Biblioteca.xocNau, 0.9f); }
    public static void AgafarXuixo()     { if (Biblioteca != null) Reproduir(Biblioteca.agafarXuixo, 0.9f); }
    public static void GameOver()        { if (Biblioteca != null) Reproduir(Biblioteca.gameOver, 1f); }

    /// <summary>Atura totes les músiques que sonen en bucle a l'escena (p. ex. en morir).</summary>
    public static void AturarMusica()
    {
        foreach (AudioSource a in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            if (a.loop && a.isPlaying) a.Stop();
    }
}
