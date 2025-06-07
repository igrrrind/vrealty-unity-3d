using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody rb;
    public Transform orientation;
    public Transform cameraPos;
    public float height;
    [Header("Movement")]
    private float x,y;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float speed;
    private Vector3 moveDirection;

    [Header("GroundCheck")]
    public float playerHeight;
    public LayerMask groundLayer;
    public float groundDrag;
    private bool isGrounded;
    private bool isSoundCoroutineRunning = false;
    private bool isRunning;
    private float soundCoroutineFreq = 2f;
    public AudioClip[] walkSoundClips;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, height - 1, cameraPos.localPosition.z);
        MyInput();
        SpeedControl();
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        if  (isGrounded) rb.linearDamping = groundDrag;
        else rb.linearDamping = 0f;

    }

    void FixedUpdate()
    {
        MovePlayer(); 
    }
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        isRunning = Input.GetKey(KeyCode.LeftShift) ? true : false;

    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * y + orientation.right * x;
        rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
        if (rb.linearVelocity != Vector3.zero && !isSoundCoroutineRunning) 
        {
            StartCoroutine(GroundEffect());
        }
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

    private void PlayRandomSFXClip(AudioClip[] soundClips)
    {
        if (soundClips == null || SFXManager.instance == null) return;

        SFXManager.instance.PlayRandomSFXClip(soundClips, transform, 1f);
    }
    private void PlaySFXClip(AudioClip soundClip)
    {
        if (soundClip == null || SFXManager.instance == null) return;
        SFXManager.instance.PlaySFXClip(soundClip, transform, 1f);
    }
    private IEnumerator GroundEffect()
    {
        isSoundCoroutineRunning = true;
        soundCoroutineFreq = isRunning ? 3f : 2f;
        PlayRandomSFXClip(walkSoundClips);
        yield return new WaitForSeconds(1f / soundCoroutineFreq);
        isSoundCoroutineRunning = false;
    }
    public void SetHeight(float level) 
    {
        height = level;
    }
}
