using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupEffectPool : MonoBehaviour
{
    public static PickupEffectPool Instance { get; private set; }

    [SerializeField] private int initialSize = 10;

    private readonly Queue<ParticleSystem> pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < initialSize; i++)
            pool.Enqueue(CreateEffect());
    }

    public void Play(Vector3 position, Color color)
    {
        ParticleSystem effect = pool.Count > 0 ? pool.Dequeue() : CreateEffect();
        effect.transform.position = position;

        ParticleSystem.MainModule main = effect.main;
        main.startColor = color;

        effect.gameObject.SetActive(true);
        effect.Play();
        StartCoroutine(ReturnAfter(effect, main.startLifetime.constantMax + 0.1f));
    }

    private ParticleSystem CreateEffect()
    {
        ParticleSystem effect = new GameObject("Pooled Pickup Spark").AddComponent<ParticleSystem>();
        effect.transform.SetParent(transform);
        effect.gameObject.SetActive(false);

        ParticleSystem.MainModule main = effect.main;
        main.loop = false;
        main.startLifetime = 0.45f;
        main.startSpeed = 1.5f;
        main.startSize = 0.11f;
        main.maxParticles = 18;

        ParticleSystem.EmissionModule emission = effect.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

        ParticleSystem.ShapeModule shape = effect.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.18f;

        return effect;
    }

    private IEnumerator ReturnAfter(ParticleSystem effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.gameObject.SetActive(false);
        pool.Enqueue(effect);
    }
}
