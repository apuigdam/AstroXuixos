using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField]
    Transform targetPlayer;

    void LateUpdate()
    {
        // La càmera segeuix l'ocell a l'eix X
        transform.position = new Vector3(targetPlayer.position.x + 7, transform.position.y, transform.position.z);
    }
}
