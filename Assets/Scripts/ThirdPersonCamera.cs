using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(1.79f, 2.02f, -3.64f);
    public float sensitivity = 1.7f;
    public float smoothSpeed = 3000f;

    [Header("Pitch Clamp")]
    public float minPitch = -35f;
    public float maxPitch = 65f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 rot = transform.eulerAngles;
        yaw = rot.y;
        pitch = rot.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!player) return;

        // ----- MOUSE LOOK -----
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // ----- CAMERA POSITION -----
        Vector3 desiredPos = player.position + rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        // ----- CAMERA ROTATION -----
        transform.rotation = rotation;
    }
}
