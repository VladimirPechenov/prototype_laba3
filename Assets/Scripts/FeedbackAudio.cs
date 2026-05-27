using UnityEngine;

public static class FeedbackAudio
{
    private static AudioClip pickupClip;
    private static AudioClip interactClip;

    public static void PlayPickup(Vector3 position)
    {
        PlayClip(ref pickupClip, position, 880f, 0.08f, 0.35f);
    }

    public static void PlayInteract(Vector3 position)
    {
        PlayClip(ref interactClip, position, 330f, 0.12f, 0.4f);
    }

    private static void PlayClip(ref AudioClip cachedClip, Vector3 position, float frequency, float duration, float volume)
    {
        cachedClip ??= CreateTone(frequency, duration);
        AudioSource.PlayClipAtPoint(cachedClip, position, volume);
    }

    private static AudioClip CreateTone(float frequency, float duration)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - (i / (float)sampleCount);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
        }

        AudioClip clip = AudioClip.Create("Feedback Tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
