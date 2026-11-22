/// <summary>
/// Guarda els resultats de l'última partida perquè es puguin mostrar
/// en una altra escena (per exemple, la pantalla de punts de la cinemàtica final).
/// Com que és "static", no es perd quan es canvia d'escena.
/// </summary>
public static class ResultatsPartida
{
    public static bool Guanyat;
    public static int Punts;
    public static int Xuixos;
    public static int XuixosTotals = 10;
    public static int Record;
    public static bool NouRecord;
}
