using UnityEngine;

public class Scenery : MonoBehaviour
{
    // Indices: PC = 0, Flip = 1, Solda = 2, Mesa = 3.
    public SpriteRenderer spriteRenderer;
    public Sprite[] images = new Sprite[4];
    public GameObject[] dialogueSets = new GameObject[4];
    private GameObject currentDialogueSet;

    private void Awake()
    {
        if (dialogueSets == null) return;
        foreach (GameObject dialogueSet in dialogueSets)
            if (dialogueSet != null) dialogueSet.SetActive(false);
    }

    // Retained for callers that supply the location and character index.
    public void OnClick(char scenery, int character)
    {
        SelectScenery(character);
    }

    // A single int parameter can also be wired directly to a Unity UI Button.
    public void SelectScenery(int index)
    {
        if (spriteRenderer == null || images == null || dialogueSets == null ||
            index < 0 || index >= images.Length || index >= dialogueSets.Length ||
            images[index] == null || dialogueSets[index] == null)
        {
            Debug.LogWarning($"Scenery index {index} is not configured.", this);
            return;
        }

        if (currentDialogueSet != null) currentDialogueSet.SetActive(false);
        spriteRenderer.sprite = images[index];
        currentDialogueSet = dialogueSets[index];
        currentDialogueSet.SetActive(true);
    }
}
