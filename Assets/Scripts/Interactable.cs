using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [SerializeField] private string promptText = "Press E";
    [SerializeField] private UnityEvent onInteract;

    private bool playerInRange;

    public UnityEvent OnInteract => onInteract;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        GameManager.Instance?.ShowPrompt(promptText);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        GameManager.Instance?.ShowPrompt(string.Empty);
    }

    private void Update()
    {
        if (!playerInRange || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        onInteract.Invoke();
        FeedbackAudio.PlayInteract(transform.position);
    }
}
