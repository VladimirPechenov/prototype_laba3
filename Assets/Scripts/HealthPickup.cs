using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 30;
    [SerializeField] private float rotationSpeed = 90f;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        health.Heal(healAmount);
        FeedbackAudio.PlayPickup(transform.position);
        Destroy(gameObject);
    }
}
