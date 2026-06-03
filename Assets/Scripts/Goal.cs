using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool requireRitualRemains = true;

    private bool completed;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (completed || !other.CompareTag("Player"))
            return;

        if (requireRitualRemains && GameManager.Instance != null && !GameManager.Instance.HasEnoughRemains())
        {
            GameManager.Instance.ShowPrompt("Collect all remains first");
            return;
        }

        ShowWin();
    }

    public void ShowWin()
    {
        completed = true;
        GameManager.Instance?.SetObjective("Ritual complete. You escaped the cemetery.");

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
