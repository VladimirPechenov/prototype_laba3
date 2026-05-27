using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private int value = 10;
    [SerializeField] private ParticleSystem collectParticles;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float bobAmplitude = 0.12f;
    [SerializeField] private float bobFrequency = 2f;

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = startPosition + Vector3.up * bob;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameManager.Instance?.AddScore(value);
        FeedbackAudio.PlayPickup(transform.position);

        if (collectParticles != null)
        {
            collectParticles.transform.SetParent(null);
            collectParticles.Play();
            Destroy(collectParticles.gameObject, collectParticles.main.duration + collectParticles.main.startLifetime.constantMax);
        }

        Destroy(gameObject);
    }
}
