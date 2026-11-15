using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    


    
    private void Awake()
    {
        dialogueInfo = new Queue<DialogueBase.Info>();
        if (instance != null)
        {

            Debug.LogWarning("Fix this" + gameObject.name);
        }
        else
        {
            instance = this;

        }
    }
    public GameObject dialogueBox;
    public GameObject dialogueOptionUI;
    public GameObject[] optionButtons;

    public TextMeshProUGUI dialogueName;
    public TextMeshProUGUI dialogueText;

    public Image[] characterPortraits;


    public float delay = 0.001f;
    private bool isDialogueOption;
    private bool isCurrentlyTyping; // Locks the active dialogue until its lines end.
    private Coroutine typing;
    private DialogueBase.Info typingLine;
    private bool waitingForChoice;
    private bool hasCompleted;
    [SerializeField] private string completedScene;
    public UnityEvent onDialogueCompleted = new UnityEvent();
    private int optionsAmount;
    public Text questionText;

    public Transform nameHolder;
    public DialogueBase currentDialogue;

    public GameObject root;
    //public Button botaodelugar;

    public Animator[] anim;


    public Queue<DialogueBase.Info> dialogueInfo; // FIFO Collection

    private void Start()
    {
        if (root != null) root.SetActive(true);
    }
    
    private GameObject[] GetAllOptionButtons()
{
    if (dialogueOptionUI == null)
    {
        Debug.LogError("dialogueOptionUI não está configurado!");
        return optionButtons ?? new GameObject[0];
    }
    
    // Busca todos os botões filhos que tenham o componente UnityEventHandler
    UnityEventHandler[] handlers = dialogueOptionUI.GetComponentsInChildren<UnityEventHandler>(true);
    GameObject[] foundButtons = new GameObject[handlers.Length];
    
    for (int i = 0; i < handlers.Length; i++)
    {
        foundButtons[i] = handlers[i].gameObject;
    }
    
    Debug.Log($"Encontrados {foundButtons.Length} botões automaticamente");
    return foundButtons;
}
    public void EnqueueDialogue(DialogueBase db)
    {
        if (db == null || isCurrentlyTyping)
        {
            return;
        }
        else
        {
            isCurrentlyTyping = true;
        }

        optionsAmount = 0;
        waitingForChoice = false;
        hasCompleted = false;
        if (dialogueOptionUI != null) dialogueOptionUI.SetActive(false);
        currentDialogue = db;
        dialogueBox.SetActive(true);
        dialogueInfo.Clear();
        SetCharacterPortraits(db);

        if (db is DialogueOptions)
        {
            isDialogueOption = true;
            DialogueOptions dialogueOptions = db as DialogueOptions;

            // Usa busca automática de botões
            GameObject[] currentOptionButtons = GetAllOptionButtons();

            if (dialogueOptions.optionsInfo == null)
            {
                Debug.LogError("optionsInfo é null no DialogueOptions!");
                isCurrentlyTyping = false;
                return;
            }

            optionsAmount = dialogueOptions.optionsInfo.Length;

            if (questionText != null)
            {
                questionText.text = dialogueOptions.questionText ?? "";
            }

            // Primeiro, desativa todos os botões
            for (int i = 0; i < currentOptionButtons.Length; i++)
            {
                if (currentOptionButtons[i] != null)
                {
                    currentOptionButtons[i].SetActive(false);
                }
            }

            // Configura apenas os botões necessários
            int maxButtons = Mathf.Min(optionsAmount, currentOptionButtons.Length);

            for (int i = 0; i < maxButtons; i++)
            {
                if (currentOptionButtons[i] == null)
                {
                    Debug.LogWarning($"Botão {i} é null!");
                    continue;
                }

                if (dialogueOptions.optionsInfo[i] == null)
                {
                    Debug.LogWarning($"optionsInfo[{i}] é null!");
                    continue;
                }

                // Ativa o botão
                currentOptionButtons[i].SetActive(true);

                // Configura o texto do botão
                Transform childTransform = currentOptionButtons[i].transform.childCount > 0
                    ? currentOptionButtons[i].transform.GetChild(0) : null;
                if (childTransform != null)
                {
                    TextMeshProUGUI buttonText = childTransform.GetComponent<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = dialogueOptions.optionsInfo[i].buttonName ?? $"Opção {i + 1}";
                    }
                    else
                    {
                        Debug.LogWarning($"TextMeshProUGUI não encontrado no botão {i}!");
                    }
                }

                // Configura o UnityEventHandler
                UnityEventHandler myEventHandler = currentOptionButtons[i].GetComponent<UnityEventHandler>();
                if (myEventHandler != null)
                {
                    myEventHandler.eventHandler = dialogueOptions.optionsInfo[i].myEvent;
                    myEventHandler.optionIndex = i;

                    if (dialogueOptions.optionsInfo[i].nextDialogue != null)
                    {
                        myEventHandler.myDialogue = dialogueOptions.optionsInfo[i].nextDialogue;
                    }
                    else
                    {
                        myEventHandler.myDialogue = null;
                    }
                }
                else
                {
                    Debug.LogWarning($"UnityEventHandler não encontrado no botão {i}!");
                }
            }

            // Avisa se há mais opções do que botões disponíveis
            if (optionsAmount > currentOptionButtons.Length)
            {
                Debug.LogWarning($"Há {optionsAmount} opções, mas apenas {currentOptionButtons.Length} botões disponíveis!");
            }
        }
        else
        {
            isDialogueOption = false;
        }

        foreach (DialogueBase.Info info in db.dialogueInfo ?? new DialogueBase.Info[0])
        {
            dialogueInfo.Enqueue(info);
        }

        DequeueDialogue();
        
    }

    public void DequeueDialogue()
    {
        if (!isCurrentlyTyping) return;
        if (typing != null)
        {
            StopCoroutine(typing);
            typing = null;
            dialogueText.text = typingLine.myText;
            return; // First click reveals the line; the next advances it.
        }
        if (dialogueInfo.Count == 0)
        {
            EndOfDialogue();
            return;
        }

        DialogueBase.Info info = dialogueInfo.Dequeue();


        nameHolder.position = characterPortraits[GetCurrentCharacterIndex(info)].gameObject.transform.position - new Vector3(0, 0);
        dialogueName.text = info.character.myName;
        dialogueText.text = info.myText;
        info.ChangeEmotion();
        //dialoguePortrait.sprite = info.character.MyPortrait;
        characterPortraits[GetCurrentCharacterIndex(info)].sprite = info.character.MyPortrait;
        DarkenOtherPortraits(info);

        if (info.character.Emotion != CharacterProfile.EmotionType.Neutro)
        {
            anim[GetCurrentCharacterIndex(info)].Play(info.characterEmotion.ToString() + "Animation");
        }


        typingLine = info;
        if (!string.IsNullOrEmpty(info.myText)) typing = StartCoroutine(TypeText(info));
    }


    private void DarkenOtherPortraits(DialogueBase.Info info)
    {
        for (int i = 0; i < characterPortraits.Length; i++)
        {
            if (i == GetCurrentCharacterIndex(info)) // this character is talking
            {
                characterPortraits[i].color = hexToColor("FFFFFF");
                characterPortraits[i].rectTransform.localScale = new Vector3(1.2f, 1.2f);
                //mudar tamanho dos personagens quando falando, cuidado com a resolução
            }
            else // this character is not talking
            {
                characterPortraits[i].color = hexToColor("9F9F9F");
                characterPortraits[i].rectTransform.localScale = new Vector3(1, 1);
            }
        }
    }



    private int GetCurrentCharacterIndex(DialogueBase.Info info)
    {
        for (int i = 0; i < currentDialogue.characters.Length; i++)
        {
            if (info.character == currentDialogue.characters[i])
            {
                return i;
            }
        }

        Debug.Log("Error! Character is not in the list!");
        return 0;
    }



    private void SetCharacterPortraits(DialogueBase db)
    {
        for (int i = 0; i < characterPortraits.Length; i++)
        {
            characterPortraits[i].sprite = db.characters[i].emotionPortraits.neutro;
        }
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            DequeueDialogue();
        }
    }

    IEnumerator TypeText(DialogueBase.Info info)
    {
        dialogueText.text = "";
        foreach (char c in info.myText.ToCharArray())
        {
            yield return new WaitForSeconds(delay);
            dialogueText.text += c;
            yield return null;
        }
        typing = null;
    }

    public void EndOfDialogue()
    {
        if (!isCurrentlyTyping) return;
        if (typing != null) StopCoroutine(typing);
        typing = null;
        isCurrentlyTyping = false;
        if (dialogueBox != null) dialogueBox.SetActive(false);
        waitingForChoice = isDialogueOption && optionsAmount > 0;
        if (dialogueOptionUI != null) dialogueOptionUI.SetActive(waitingForChoice);
        if (!waitingForChoice) CompleteDialogue();
    }

    public void DeactivateAllOptionButtons()
    {
        if (dialogueOptionUI != null) dialogueOptionUI.SetActive(false);
    }

    public void SelectOption(int index)
    {
        if (!waitingForChoice || !(currentDialogue is DialogueOptions options) ||
            options.optionsInfo == null || index < 0 || index >= options.optionsInfo.Length)
            return;
        DialogueOptions.Options option = options.optionsInfo[index];
        if (option == null) return;
        waitingForChoice = false; // Ignore repeated pointer events.
        DeactivateAllOptionButtons();
        if (option.increasesAffection && option.targetCharacter != null)
            option.targetCharacter.aumentoAfeto();
        option.myEvent?.Invoke();
        // An event may itself start dialogue or change the scene.
        if (this == null || isCurrentlyTyping) return;
        if (option.nextDialogue != null) EnqueueDialogue(option.nextDialogue);
        else CompleteDialogue();
    }

    private void CompleteDialogue()
    {
        if (hasCompleted) return;
        hasCompleted = true;
        string destination = currentDialogue != null && !string.IsNullOrWhiteSpace(currentDialogue.nextScene)
            ? currentDialogue.nextScene : completedScene;
        onDialogueCompleted.Invoke();
        if (this == null || isCurrentlyTyping || string.IsNullOrWhiteSpace(destination)) return;
        if (Application.CanStreamedLevelBeLoaded(destination)) SceneManager.LoadScene(destination);
        else Debug.LogWarning($"Dialogue destination '{destination}' is not enabled in the build.");
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public Color hexToColor(string hex)
    {
        hex = hex.Replace("0x", ""); // In case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");  // In case the string is formatted #FFFFFF
        byte a = 255; // Assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        // Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return new Color32(r, g, b, a);
    }
}