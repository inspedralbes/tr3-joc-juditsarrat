using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private VisualElement _root;
    private Label _winnerLabel;
    private Button _btnReiniciar;
    private Button _btnLobby;

    private void OnEnable()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) {
            Debug.LogError("[GameOverUI] ❌ No s'ha trobat el component UIDocument en aquest GameObject!");
            return;
        }

        if (doc.panelSettings == null) {
            Debug.LogWarning("[GameOverUI] ⚠️ El UIDocument no té Panel Settings assignats! Assigna 'MainPanelSetting' a l'Inspector.");
        }

        if (doc.visualTreeAsset == null) {
            Debug.LogError("[GameOverUI] ❌ El UIDocument no té cap UXML (Source Asset) assignat!");
            return;
        }

        _root = doc.rootVisualElement;
        if (_root == null) {
            Debug.LogError("[GameOverUI] ❌ El rootVisualElement és nul!");
            return;
        }

        _winnerLabel = _root.Q<Label>("WinnerMessage");
        _btnReiniciar = _root.Q<Button>("btn-reiniciar");
        _btnLobby = _root.Q<Button>("btn-lobby");

        // Configurar text del guanyador
        if (_winnerLabel != null)
        {
            string winner = GameOverData.WinnerName;
            if (string.IsNullOrEmpty(winner)) winner = "Ningú";
            _winnerLabel.text = winner + " ha guanyat la partida!";
            Debug.Log($"[GameOverUI] ✅ Text del guanyador establert: {winner}");
        }
        else {
            Debug.LogWarning("[GameOverUI] ⚠️ No s'ha trobat el Label 'WinnerMessage' al UXML. Revisa el nom!");
        }

        // Callbacks dels botons
        if (_btnReiniciar != null) {
            _btnReiniciar.clicked += OnReiniciarClicked;
            Debug.Log("[GameOverUI] ✅ Botó reiniciar trobat i vinculat.");
        } else {
            Debug.LogWarning("[GameOverUI] ⚠️ No s'ha trobat el Botó 'btn-reiniciar' al UXML.");
        }

        if (_btnLobby != null) {
            _btnLobby.clicked += OnLobbyClicked;
            Debug.Log("[GameOverUI] ✅ Botó lobby trobat i vinculat.");
        } else {
            Debug.LogWarning("[GameOverUI] ⚠️ No s'ha trobat el Botó 'btn-lobby' al UXML.");
        }
    }

    private void OnReiniciarClicked()
    {
        Debug.Log("[GameOverUI] Reiniciant partida...");
        // Carrega l'escena de joc
        SceneManager.LoadScene("GameScene"); 
    }

    private void OnLobbyClicked()
    {
        Debug.Log("[GameOverUI] Tornant al lobby...");
        // Carrega l'escena del menú principal o lobby (ajusta el nom segons el teu projecte)
        SceneManager.LoadScene("MenuPrincipal"); 
    }
}
