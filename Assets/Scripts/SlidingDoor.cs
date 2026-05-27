using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [SerializeField] private Vector3 openOffset = new(0f, 3f, 0f);
    [SerializeField] private float moveSpeed = 4f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;
    private bool isOpen;

    private void Awake()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        targetPosition = isOpen ? closedPosition + openOffset : closedPosition;
    }
}
