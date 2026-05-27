using UnityEngine;

public class RitualAltar : MonoBehaviour
{
    [SerializeField] private Light ritualLight;
    [SerializeField] private ParticleSystem ritualParticles;

    private bool completed;

    public void TryCompleteRitual()
    {
        if (completed)
            return;

        if (GameManager.Instance == null || !GameManager.Instance.HasEnoughRemains())
        {
            GameManager.Instance?.ShowPrompt("Need more remains");
            return;
        }

        completed = true;
        GameManager.Instance.SetObjective("Ritual complete. Prototype finished.");
        FeedbackAudio.PlayInteract(transform.position);

        if (ritualLight != null)
        {
            ritualLight.enabled = true;
            ritualLight.intensity = 6f;
        }

        if (ritualParticles != null)
            ritualParticles.Play();
    }
}
