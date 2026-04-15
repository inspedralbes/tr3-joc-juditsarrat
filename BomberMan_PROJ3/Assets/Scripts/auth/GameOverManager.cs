using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    private Label _winnerLabel;
    private Button _newGameButton;
    private Button _lobbyButton;

    void OnEnable()
    {
        // Obtener la raíz del documento UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // Asignar elementos buscando por el nombre (ID) definido en el UXML
        _winnerLabel = root.Q<Label>("WinnerNameText");

        string nomFinal = PlayerPrefs.GetString("UltimGuanyador", "Algú");
        ActualitzarTextGuanyador(nomFinal);
         
        _newGameButton = root.Q<Button>("NewGameButton");
        _lobbyButton = root.Q<Button>("LobbyButton");

        // Suscribir eventos de clic
        _newGameButton.clicked += OnNewGameClicked;
        _lobbyButton.clicked += OnLobbyClicked;
    }


public void ActualitzarTextGuanyador(string nomGuanyador)
    {
        if (_winnerLabel != null)
        {
            // La frase que demanes:
            _winnerLabel.text = $"{nomGuanyador} ha guanyat !!";
        }
    }
    
    // Método para actualizar el nombre del ganador desde otros scripts
    public void SetWinner(string winnerName)
    {
        if (_winnerLabel != null)
            _winnerLabel.text = "Guanyador: " + winnerName;
    }

    private void OnNewGameClicked()
    {
        Debug.Log("Reiniciando partida...");
        // SceneManager.LoadScene("GameScene"); // Ajusta el nombre de tu escena
    }

    private void OnLobbyClicked()
    {
        Debug.Log("Volviendo al lobby...");
        // SceneManager.LoadScene("LobbyScene"); // Ajusta el nombre de tu escena
    }

    void OnDisable()
    {
        // Es buena práctica desuscribir eventos en UI Toolkit
        _newGameButton.clicked -= OnNewGameClicked;
        _lobbyButton.clicked -= OnLobbyClicked;
    }
}