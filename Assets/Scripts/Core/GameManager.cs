using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

/*
Os metodos e variaveis dessa classe podem ser acessados de qualquer parte do codigo a qualquer momento
Para acessar, basta utilizar:

GameManager.Instance.variavel ou GameManager.Instance.metodo
*/

public class GameManager : MonoBehaviour
{
    // Instancia singular
    public static GameManager Instance;

    // Informações do jogador
    public string playerName;
    public int affectionPoints;
    public int currentDay = 1;

    // Gênero selecionado
    public string selectedGender;

    // Caixa de diálogo selecionada
    public GameObject selectedDialogueBox;

    // Sprite para personagem que deve receber um gênero 
    public Sprite playerSprite;

    private void Awake()
    {
        // Afirma que apenas uma instancia do GameManager pode existir
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //Verifica se o Loading foi instanciado
        if(GameObject.Find("Loading") == null) {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Loading");
            if (prefab != null)
            {
                GameObject loading = Instantiate(prefab);
                loading.name = "Loading";
            }
        }

    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void LoadNextScene(string NextScene)
    {
        // Carrega a próxima cena
        SceneManager.LoadScene(NextScene);
    }

    // Método que deve ser chamado no fim de cada dia para salvar o jogo (Pode ser substituído por "SaveManager.Instance.SaveGame();" diretamente)
    public void OnDayEnd()
    {
        currentDay = 5; // The next implemented day after day 1.
        SaveManager.Instance.SaveDayEnd(currentDay);
    }

    // Método para carregar as informações salvas
    public void LoadData(SaveData data)
    {
        if (data == null) return;
        currentDay = data.day;
        this.playerName = data.playerName;
        this.affectionPoints = data.affectionPoints;
        this.selectedGender = data.selectedGender;
        // Appearance is rebound by each scene from the saved gender identifier.
        selectedDialogueBox = null;
        playerSprite = null;
        // SaveManager.OnPlay restores the checkpoint scene after loading this state.
    }

    // Método para atualizar as informações salvas
    public void SaveData(ref SaveData data)
    {
        if (data == null) data = new SaveData();
        SavePlayerInfo(ref data);
        data.affectionPoints = affectionPoints;
        data.day = currentDay;
    }

    // Método para atualizar as informações do jogador (nome e gênero)
    public void SavePlayerInfo(ref SaveData data)
    {
        if (data == null) data = new SaveData();
        data.playerName = this.playerName;
        data.selectedGender = this.selectedGender;

    }
}