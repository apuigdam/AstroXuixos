using UnityEngine;

/// <summary>
/// Llista de tots els sons del joc. N'hi ha una sola a Assets/Resources/BibliotecaSons.asset
/// (la crea l'eina AstroXuixos > Configurar sons). Per canviar un so, arrossegueu-lo aquí a l'Inspector.
/// </summary>
[CreateAssetMenu(menuName = "AstroXuixos/Biblioteca de sons", fileName = "BibliotecaSons")]
public class BibliotecaSons : ScriptableObject
{
    [Header("Música")]
    public AudioClip musicaMenu;

    [Header("Menú")]
    public AudioClip clicBoto;
    public AudioClip passarPerSobreBoto;

    [Header("Joc")]
    public AudioClip dispar;
    public AudioClip explosioMeteorit;
    public AudioClip xocNau;
    public AudioClip agafarXuixo;
    public AudioClip gameOver;
}
