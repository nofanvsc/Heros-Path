using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 2f, -4f);
    public float sensitivity = 5f;
    public float smoothSpeed = 10f;

    private float yaw, pitch;

    void LateUpdate()
    {
        // Mouse movement to rotate the camera freely
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -35f, 60f); // limit up/down angle

        // Compute camera position and rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = player.position + rotation * offset;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}
