using UnityEngine;

public class GraveDigSpot : MonoBehaviour
{
    [SerializeField] private int remainsValue = 1;
    [SerializeField] private GameObject hiddenRemains;
    [SerializeField] private Material completedMaterial;

    private bool dug;

    public void Dig()
    {
        if (dug)
            return;

        dug = true;
        GameManager.Instance?.AddRitualRemains(remainsValue);
        FeedbackAudio.PlayPickup(transform.position);

        if (hiddenRemains != null)
            hiddenRemains.SetActive(true);

        if (completedMaterial != null && TryGetComponent(out Renderer renderer))
            renderer.sharedMaterial = completedMaterial;
    }
}
