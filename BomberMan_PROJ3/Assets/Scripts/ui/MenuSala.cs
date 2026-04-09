using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class MenuSala : MonoBehaviour
{
    private VisualElement _seccionMenu;
    private VisualElement _seccionLobby;
    private Button _botoCrearSala;
    private Label _labelCodiSala;
    private GameLobbyUI _lobbyUI;

    private string _currentGameId;
    private string _currentGameCode;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _seccionMenu = root.Q<VisualElement>("seccion-menu");
        _seccionLobby = root.Q<VisualElement>("seccion-lobby");
        _botoCrearSala = root.Q<Button>("btn-crear");
        _labelCodiSala = root.Q<Label>("label-codi-sala");
        
        _lobbyUI = GetComponent<GameLobbyUI>();

        if (_botoCrearSala != null)
        {
            _botoCrearSala.clicked += OnClickCrearSala;
        }
    }

    private void OnClickCrearSala()
    {
        Debug.Log("🎮 Creant sala...");
        StartCoroutine(GameService.Instance.CreateGameRoom(
            onSuccess: (responseJson) =>
            {
                Debug.Log("✅ Resposta: " + responseJson);

                try
                {
                    GameResponse game = JsonUtility.FromJson<GameResponse>(responseJson);
                    string _id = game._id;

                    if (string.IsNullOrEmpty(_id))
                    {
                        Debug.LogError("❌ ID no encontrado en la respuesta");
                        return;
                    }

                    _currentGameId = _id;
                    _currentGameCode = GameCodeGenerator.GenerateCode(_id);

                    MostrarLobby();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("❌ Error: " + e.Message);
                }
            },
            onError: (error) =>
            {
                Debug.LogError("❌ Error: " + error);
            }
        ));
    }

    private void MostrarLobby()
    {
        _seccionMenu.style.display = DisplayStyle.None;
        _seccionLobby.style.display = DisplayStyle.Flex;
        _labelCodiSala.text = _currentGameCode;

        if (_lobbyUI != null)
        {
            _lobbyUI.SetGameInfo(_currentGameId);
        }

        Debug.Log("✅ Lobby mostrat!");
    }

    private void OnDisable()
    {
        if (_botoCrearSala != null)
        {
            _botoCrearSala.clicked -= OnClickCrearSala;
        }
    }
}