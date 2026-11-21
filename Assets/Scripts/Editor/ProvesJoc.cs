using UnityEditor;
using UnityEngine;

/// <summary>
/// Opcions de prova per al joc. Només funcionen en mode Play a l'escena Game.
/// Menú de Unity: AstroXuixos > Proves (en Play) > ...
/// </summary>
public static class ProvesJoc
{
    const string Menu = "AstroXuixos/Proves (en Play)/";

    static bool EnPartida() => Application.isPlaying && GameManager.Instance != null;

    [MenuItem(Menu + "Agafar un xuixo (efecte a l'atzar)")]
    static void AgafarXuixo() => GameManager.Instance.XuixoRecollit();
    [MenuItem(Menu + "Agafar un xuixo (efecte a l'atzar)", true)]
    static bool AgafarXuixoValid() => EnPartida();

    [MenuItem(Menu + "Perdre una vida")]
    static void PerdreVida() => GameManager.Instance.RebreDany();
    [MenuItem(Menu + "Perdre una vida", true)]
    static bool PerdreVidaValid() => EnPartida();

    [MenuItem(Menu + "Game over")]
    static void GameOver() => GameManager.Instance.ForcarGameOver();
    [MenuItem(Menu + "Game over", true)]
    static bool GameOverValid() => EnPartida();
}
