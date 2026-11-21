using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Posa'l al mateix GameObject que el Video Player de la cinemàtica.
/// - Quan el vídeo s'acaba, carrega l'escena següent.
/// - Si prems ESC (o Start al comandament), se salta la cinemàtica.
/// Serveix per la CinematicaInicial (-> Game) i per la CinematicaFinal (-> Menu),
/// només cal canviar el camp "escenaSeguent" a l'Inspector.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class ControladorCinematica : MonoBehaviour
{
    [Tooltip("Escena que es carrega quan acaba (o se salta) el vídeo")]
    public string escenaSeguent = "Game";

    [Tooltip("Permetre saltar la cinemàtica amb ESC")]
    public bool esPotSaltar = true;

    private VideoPlayer video;
    private bool canviant = false;

    void Awake()
    {
        video = GetComponent<VideoPlayer>();
        video.isLooping = false;               // si fes bucle, no s'acabaria mai
    }

    void OnEnable()
    {
        // loopPointReached = "el vídeo ha arribat al final"
        video.loopPointReached += VideoAcabat;
        // Si el vídeo dona error (p. ex. format no suportat), no ens quedem penjats
        video.errorReceived += ErrorVideo;
    }

    void OnDisable()
    {
        video.loopPointReached -= VideoAcabat;
        video.errorReceived -= ErrorVideo;
    }

    void Update()
    {
        if (!esPotSaltar) return;

        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool start = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

        if (esc || start)
            AnarEscenaSeguent();
    }

    private void VideoAcabat(VideoPlayer vp)
    {
        AnarEscenaSeguent();
    }

    private void ErrorVideo(VideoPlayer vp, string missatge)
    {
        Debug.LogWarning("[Cinematica] Error amb el vídeo: " + missatge + ". Passem a l'escena següent.");
        AnarEscenaSeguent();
    }

    private void AnarEscenaSeguent()
    {
        if (canviant) return;   // evita carregar l'escena dues vegades
        canviant = true;

        video.Stop();
        SceneManager.LoadScene(escenaSeguent);
    }
}
