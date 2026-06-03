using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        Coins,
        KeyFragment,
        Healing
    }

    [SerializeField] private PickupType pickupType = PickupType.Coins;
    [SerializeField] private int value = 10;
    [SerializeField] private int healAmount = 20;
    [SerializeField] private float rotationSpeed = 95f;
    [SerializeField] private float bobAmplitude = 0.12f;
    [SerializeField] private float bobFrequency = 2f;

    private Vector3 startPosition;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        startPosition = transform.position;
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

        switch (pickupType)
        {
            case PickupType.Coins:
                ResourceManager.Instance?.AddCoins(value, transform.position);
                PickupEffectPool.Instance?.Play(transform.position, new Color(1f, 0.78f, 0.16f));
                break;
            case PickupType.KeyFragment:
                ResourceManager.Instance?.AddKeyFragments(value, transform.position);
                PickupEffectPool.Instance?.Play(transform.position, new Color(0.55f, 0.85f, 1f));
                break;
            case PickupType.Healing:
                other.GetComponent<PlayerHealth>()?.Heal(healAmount);
                ResourceManager.Instance?.ShowFeedback($"+{healAmount} HP");
                FeedbackAudio.PlayPickup(transform.position);
                PickupEffectPool.Instance?.Play(transform.position, new Color(0.1f, 0.75f, 0.35f));
                break;
        }

        Destroy(gameObject);
    }
}
