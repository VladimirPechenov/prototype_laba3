using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float defaultDuration = 0.18f;
    [SerializeField] private float defaultAmplitude = 0.08f;

    private Vector3 startLocalPosition;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultAmplitude);
    }

    public void Shake(float duration, float amplitude)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, amplitude));
    }

    private IEnumerator ShakeRoutine(float duration, float amplitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float fade = 1f - elapsed / duration;
            Vector2 offset = Random.insideUnitCircle * amplitude * fade;
            transform.localPosition = startLocalPosition + new Vector3(offset.x, offset.y, 0f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = startLocalPosition;
        shakeRoutine = null;
    }
}
