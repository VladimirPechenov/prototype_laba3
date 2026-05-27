using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraLook : MonoBehaviour
{
    [SerializeField] private float pitchSensitivity = 0.08f;
    [SerializeField] private float minPitch = -70f;
    [SerializeField] private float maxPitch = 75f;

    private float pitch;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        pitch = Mathf.Clamp(pitch - Mouse.current.delta.ReadValue().y * pitchSensitivity, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
