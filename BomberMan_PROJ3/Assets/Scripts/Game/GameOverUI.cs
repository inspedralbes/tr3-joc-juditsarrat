using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private void OnEnable()
    {
        // 1. Buscamos el componente en el mismo objeto
        UIDocument uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;

        VisualElement root = uiDoc.rootVisualElement;

        // 2. Buscamos los botones por el nombre exacto del UXML
        Button btnReiniciar = root.Q<Button>("btn-reiniciar");
        Button btnLobby = root.Q<Button>("btn-lobby");

        // 3. Asignamos los eventos
        if (btnReiniciar != null)
        {
            btnReiniciar.clicked += () => SceneManager.LoadScene("GameScene");
        }

        if (btnLobby != null)
        {
            btnLobby.clicked += () => SceneManager.LoadScene("LobbyScene");
        }
    }
}