using UnityEngine;
using UnityEngine.UI;

public class SelecaoLugarDia1 : MonoBehaviour
{
    [SerializeField] private Button b1, b2, b3, b4;
    public static char selec = 'N';

    private void Awake()
    {
        Configure(b1, 'F');
        Configure(b2, 'M');
        Configure(b3, 'S');
        Configure(b4, 'P');
    }

    private void Configure(Button button, char location)
    {
        if (button == null) return;
        button.interactable = SceneRoutes.IsEnabled(SceneRoutes.Location(1, location));
        button.onClick.AddListener(() => Selecionar(location));
    }

    private void Selecionar(char location)
    {
        string destination = SceneRoutes.Location(1, location);
        if (SceneRoutes.IsEnabled(destination))
        {
            selec = location;
            SceneRoutes.Load(destination);
        }
    }
}
