using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Material activatedMaterial;

    private bool activated;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        activated = true;
        Vector3 respawnPosition = transform.position + Vector3.up;
        health.SetRespawnPoint(respawnPosition);
        SaveSystem.Instance?.SaveCheckpoint(respawnPosition);
        FeedbackAudio.PlayInteract(transform.position);

        if (activatedMaterial != null && TryGetComponent(out Renderer renderer))
            renderer.sharedMaterial = activatedMaterial;
    }
}
