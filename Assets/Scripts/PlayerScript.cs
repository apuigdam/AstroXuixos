using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] float forwardSpeed = 2.0f;
    [SerializeField] float verticalSpeed = 5.0f;

    Rigidbody2D playerBody;

    void Start()
    {
        playerBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float vertical = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            vertical = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            vertical = -1f;
        }

        playerBody.linearVelocity = new Vector2(forwardSpeed, vertical * verticalSpeed);
    }
}