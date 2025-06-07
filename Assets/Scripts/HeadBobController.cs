using UnityEngine;

public class HeadbobController : MonoBehaviour
{
    [Header("Headbob Settings")]
    public float walkBobSpeed = 5f;   // Frequency of bobbing (higher = faster)
    public float walkBobAmountX = 0.05f; // Horizontal sway amount
    public float walkBobAmountY = 0.1f;  // Vertical bounce amount
    public float speed;
    private float timer = 0f;
    private Vector3 originalPosition;
    public GameObject player;
    private Rigidbody playerRigidbody;
    public Transform camera;
    public bool autoBob = false;

    void Start()
    {
        originalPosition = camera.transform.localPosition;
        if (player != null)
        playerRigidbody = player.GetComponent<Rigidbody>();
    }

    void Update()
    {
        ApplyHeadbob();
    }

    void ApplyHeadbob()
    {
        if (autoBob)
        {
            timer += Time.deltaTime * walkBobSpeed;
            float bobX = Mathf.Cos(timer) * walkBobAmountX;
            float bobY = Mathf.Sin(timer * 2) * walkBobAmountY; // Multiply timer for different Y frequency

            // Apply movement to camera
            camera.transform.localPosition = originalPosition + new Vector3(bobX, bobY, 0);
        }
        else
        {
            if (playerRigidbody == null)
                return;

            speed = playerRigidbody.linearVelocity.magnitude;

            if (speed > 0.1f) // Moving threshold
            {
                timer += Time.deltaTime * walkBobSpeed * speed;

                // Calculate headbob using sin and cos
                float bobX = Mathf.Cos(timer) * walkBobAmountX;
                float bobY = Mathf.Sin(timer * 2) * walkBobAmountY; // Multiply timer for different Y frequency

                // Apply movement to camera
                camera.transform.localPosition = originalPosition + new Vector3(bobX, bobY, 0);
            }
            else
            {
                // Reset when idle
                timer = 0;
                camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, originalPosition, Time.deltaTime * 5f);
            }
        }

    }
}