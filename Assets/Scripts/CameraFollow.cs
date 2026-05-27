using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new(0f, 2.2f, -5f);
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float pitchSensitivity = 0.08f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 45f;

    private float pitch = 12f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdatePitch();

        Quaternion yawRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
        Quaternion pitchRotation = Quaternion.Euler(pitch, 0f, 0f);
        Vector3 desiredPosition = target.position + yawRotation * pitchRotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }

    private void UpdatePitch()
    {
        if (Mouse.current == null)
            return;

        float mouseY = Mouse.current.delta.ReadValue().y;
        pitch = Mathf.Clamp(pitch - mouseY * pitchSensitivity, minPitch, maxPitch);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
