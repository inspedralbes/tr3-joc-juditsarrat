using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class GameLobbyManager : MonoBehaviour
{
    private string _currentGameId;
    private string _currentGameCode;
    private int _playerCount = 1;
    
    private Label _labelJugadores;
    private Button _botoIniciar;
    
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _labelJugadores = root.Q<Label>("label-espectadors");
        _botoIniciar = root.Q<Button>("btn-iniciar");
        
        if (_botoIniciar != null) {
            _botoIniciar.clicked += OnClickIniciar;
            _botoIniciar.SetEnabled(false);
        }
    }

    public void SetGameInfo(string gameId, string gameCode)
    {
        _currentGameId = gameId;
        _currentGameCode = gameCode;
        _playerCount = 1;
        
        ActualizarJugadores();
        IniciarSincronizacion();
    }

    private void ActualizarJugadores()
    {
        if (_labelJugadores != null) {
            _labelJugadores.text = _playerCount + "/2 jugadores conectados";
        }
        
        if (_botoIniciar != null) {
            _botoIniciar.SetEnabled(_playerCount >= 2);
        }
        
        Debug.Log("Jugadores: " + _playerCount + "/2");
    }

    private void IniciarSincronizacion()
    {
        InvokeRepeating(nameof(SincronizarJugadores), 1f, 2f);
    }

    private void SincronizarJugadores()
    {
        if (string.IsNullOrEmpty(_currentGameId)) return;
        
        StartCoroutine(GameService.Instance.GetGameStatus(_currentGameId,
            onSuccess: (gameDataJson) => {
                try {
                    string playersCount = ExtractPlayersCount(gameDataJson);
                    int count = int.Parse(playersCount);
                    
                    if (count != _playerCount) {
                        _playerCount = count;
                        ActualizarJugadores();
                        
                        if (_playerCount >= 2) {
                            Debug.Log(" ¡Segundo jugador conectado!");
                        }
                    }
                    
                } catch (System.Exception e) {
                    Debug.LogError("Error sincronizando: " + e.Message);
                }
            },
            onError: (error) => {
                Debug.LogError("Error obteniendo estado: " + error);
            }
        ));
    }

    private string ExtractPlayersCount(string json)
    {
        int playersStart = json.IndexOf("\"players\":[");
        if (playersStart == -1) return "1";
        
        int playersEnd = json.IndexOf("]", playersStart);
        string playersJson = json.Substring(playersStart, playersEnd - playersStart + 1);
        
        int count = playersJson.Split('"').Length / 2 - 1;
        return count.ToString();
    }

    private void OnClickIniciar()
    {
        if (_playerCount < 2) {
            Debug.LogError("Necesitas 2 jugadores para iniciar");
            return;
        }
        
        Debug.Log("▶️ Iniciando partida: " + _currentGameId);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(SincronizarJugadores));
        if (_botoIniciar != null) {
            _botoIniciar.clicked -= OnClickIniciar;
        }
    }
}