using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Trap : MonoBehaviour
{
    [SerializeField] private int damage = 25;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private bool pulseWarning = true;
    [SerializeField] private Color safeColor = new(0.35f, 0.05f, 0.05f);
    [SerializeField] private Color warningColor = new(1f, 0.1f, 0.05f);

    private float lastDamageTime = -999f;
    private Renderer cachedRenderer;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        Collider trapCollider = GetComponent<Collider>();
        trapCollider.isTrigger = true;
        cachedRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (!pulseWarning || cachedRenderer == null)
            return;

        float pulse = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
        propertyBlock.SetColor("_BaseColor", Color.Lerp(safeColor, warningColor, pulse));
        propertyBlock.SetColor("_Color", Color.Lerp(safeColor, warningColor, pulse));
        cachedRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || Time.time < lastDamageTime + cooldown)
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        health.TakeDamage(damage);
        lastDamageTime = Time.time;
    }
}
