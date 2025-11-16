using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRGamepadMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("References")]
    public Transform orientation;   // usually your PlayerRoot rotation object
    public Transform head;          // VR camera (Main Camera)
    public Transform cameraPos;     // optional camera height adjust

    [Header("Settings")]
    public float height = 1.7f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    private float speed;

    [Header("Physics")]
    public float playerHeight = 1.8f;
    public LayerMask groundLayer;
    public float groundDrag = 4f;
    private bool isGrounded;

    private bool isRunning;
    private Vector3 moveDirection;

    [Header("Footsteps")]
    public AudioClip[] walkSoundClips;
    private bool isSoundCoroutineRunning = false;
    private float soundCoroutineFreq = 2f;

    private Gamepad pad;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        pad = Gamepad.current;
        if (pad == null) return;

        // VR camera height adjust
        cameraPos.localPosition = new Vector3(
            cameraPos.localPosition.x,
            height - 1f,
            cameraPos.localPosition.z
        );

        HandleInput();
        SpeedControl();

        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, groundLayer);

        rb.linearDamping = isGrounded ? groundDrag : 0f;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        Vector2 move = pad.leftStick.ReadValue();

        float x = move.x;
        float y = move.y;

        // Run toggle: hold L2
        isRunning = (pad.leftTrigger.ReadValue() > 0.5f);

        speed = isRunning ? runSpeed : walkSpeed;

        // Move direction based on VR camera forward
        Vector3 forward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        Vector3 right   = new Vector3(head.right.x, 0, head.right.z).normalized;

        moveDirection = forward * y + right * x;

        // Smooth turn (right stick)
        float turn = pad.rightStick.ReadValue().x;
        transform.Rotate(0, turn * 65f * Time.deltaTime, 0);
    }

    private void MovePlayer()
    {
        if (!isGrounded) return;

        rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);

        // Play footsteps
        if (rb.linearVelocity.magnitude > 0.1f && !isSoundCoroutineRunning)
            StartCoroutine(GroundEffect());
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private IEnumerator GroundEffect()
    {
        isSoundCoroutineRunning = true;

        // Frequency depends on walking or running
        soundCoroutineFreq = isRunning ? runSpeed : walkSpeed;

        PlayRandomSFXClip(walkSoundClips);

        yield return new WaitForSeconds(1f / soundCoroutineFreq);
        isSoundCoroutineRunning = false;
    }

    private void PlayRandomSFXClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || SFXManager.instance == null) return;
        SFXManager.instance.PlayRandomSFXClip(clips, transform, 1f);
    }
}
