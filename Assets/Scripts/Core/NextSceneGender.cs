using UnityEngine;

public class NextSceneGender : MonoBehaviour
{
    [SerializeField] private GameObject maleDialogueBox;
    [SerializeField] private GameObject femaleDialogueBox;
    [SerializeField] private Sprite maleSprite;
    [SerializeField] private Sprite femaleSprite;

    private void Start()
    {
        if (GameManager.Instance == null) return;
        bool male = GameManager.Instance.selectedGender == "Male";
        GameManager.Instance.selectedDialogueBox = male ? maleDialogueBox : femaleDialogueBox;
        GameManager.Instance.playerSprite = male ? maleSprite : femaleSprite;
    }
}
