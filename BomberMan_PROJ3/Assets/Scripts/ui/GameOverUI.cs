using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private VisualElement _root;
    private Label _winnerLabel;
    private Button _btnLobby;

    private void OnEnable()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) return;

        _root = doc.rootVisualElement;
        if (_root == null) return;

        _winnerLabel = _root.Q<Label>("WinnerMessage");
        _btnLobby = _root.Q<Button>("btn-lobby");

        // Configurar text del guanyador / perdedor
        if (_winnerLabel != null)
        {
            if (GameOverData.IsLocalWinner)
            {
                _winnerLabel.text = "HAS GUANYAT!";
                _winnerLabel.style.color = new StyleColor(new Color(0.2f, 1f, 0.2f)); // Verd
            }
            else
            {
                _winnerLabel.text = "¡HAS MORT!";
                _winnerLabel.style.color = new StyleColor(new Color(1f, 0.2f, 0.2f)); // Vermell
            }
        }

        if (_btnLobby != null)
        {
            _btnLobby.clicked += OnLobbyClicked;
        }
    }

    private void OnDisable()
    {
        if (_btnLobby != null)
        {
            _btnLobby.clicked -= OnLobbyClicked;
        }
    }

    private void OnLobbyClicked()
    {
        // Al volver al menú, cerramos conexión si fuera necesario, 
        // pero WebSocketManager ya se encarga o persiste.
        SceneManager.LoadScene("MenuPrincipal"); 
    }
}
