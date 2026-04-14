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
                    _currentGameCode = game.gameCode; // Usa el código del servidor

                    MostrarLobby(game.players != null ? game.players.Length : 1);
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

    private void MostrarLobby(int initialPlayerCount = 1)
    {
        _seccionMenu.style.display = DisplayStyle.None;
        _seccionLobby.style.display = DisplayStyle.Flex;
        _labelCodiSala.text = _currentGameCode;

        if (_lobbyUI != null)
        {
            _lobbyUI.SetGameInfo(_currentGameId, initialPlayerCount);
        }

        Debug.Log("✅ Lobby mostrat!");
    }

    public void JoinExistingGame(string gameId, string gameCode)
    {
        _currentGameId = gameId;
        _currentGameCode = gameCode;
        
        // Obtenemos el estado actual para saber cuántos jugadores hay
        StartCoroutine(GameService.Instance.GetGameStatus(gameId, (responseJson) => {
            GameResponse game = JsonUtility.FromJson<GameResponse>(responseJson);
            _labelCodiSala.text = _currentGameCode;
            _seccionMenu.style.display = DisplayStyle.None;
            _seccionLobby.style.display = DisplayStyle.Flex;
            
            if (_lobbyUI != null) {
                _lobbyUI.SetGameInfo(_currentGameId, game.players.Length);
            }
        }));
    }

    private void OnDisable()
    {
        if (_botoCrearSala != null)
        {
            _botoCrearSala.clicked -= OnClickCrearSala;
        }
    }
}