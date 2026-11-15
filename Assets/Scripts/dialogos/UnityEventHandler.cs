using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UnityEventHandler : MonoBehaviour, IPointerDownHandler
{
    // Retained for existing serialized scene references; selection uses the current option data.
    public UnityEvent eventHandler;
    public DialogueBase myDialogue;
    public int optionIndex;

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.SelectOption(optionIndex);
    }
}
