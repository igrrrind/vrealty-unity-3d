using UnityEngine;

public class Movement : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform orientation;
    public float xRotation = 0f;
    public float yRotation = 0f;

    public PauseMenu pauseMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //this lock the cursor at the middle of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void FixedUpdate()
    {


        if (pauseMenu.isPaused)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            xRotation -= mouseY;
            yRotation += mouseX;

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}
