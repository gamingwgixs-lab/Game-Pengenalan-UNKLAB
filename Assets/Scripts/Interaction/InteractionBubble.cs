using UnityEngine;

public class InteractionBubble : MonoBehaviour
{
    [SerializeField] GameObject interactionBubble;

    private void Awake()
    {
        if (interactionBubble != null)
        {
            interactionBubble.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && interactionBubble != null)
        {
            interactionBubble.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && interactionBubble != null)
        {
            interactionBubble.SetActive(false);
        }
    }
}
