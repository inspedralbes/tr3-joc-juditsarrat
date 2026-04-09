using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class GameLobbyUI : MonoBehaviour
{
    private string _currentGameId;
    private int _playerCount = 1;
    private Label _labelJugadores;
    private Button _botoIniciar;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _labelJugadores = root.Q<Label>("label-espectadors");
        _botoIniciar = root.Q<Button>("btn-iniciar");

        if (_botoIniciar != null)
        {
            _botoIniciar.clicked += OnClickIniciar;
            _botoIniciar.SetEnabled(false);
        }
    }

    public void SetGameInfo(string gameId)
    {
        _currentGameId = gameId;
        _playerCount = 1;
        ActualizarJugadores();
        
        // ✅ Conectar WebSocket
        var wsManager = WebSocketManager.GetOrCreate();
        wsManager.OnPlayerCountChanged += OnPlayerCountChanged;
        wsManager.ConnectToGame(gameId);
    }

    private void OnPlayerCountChanged(int count)
    {
        _playerCount = count;
        ActualizarJugadores();
    }

    private void ActualizarJugadores()
    {
        if (_labelJugadores != null)
        {
            _labelJugadores.text = _playerCount + "/2 jugadores conectados";
        }

        if (_botoIniciar != null)
        {
            _botoIniciar.SetEnabled(_playerCount >= 2);
        }

        Debug.Log("👥 Jugadores: " + _playerCount + "/2");
    }

    private void OnClickIniciar()
    {
        if (_playerCount < 2)
        {
            Debug.LogError("❌ Necesitas 2 jugadores");
            return;
        }

        Debug.Log("▶️ Iniciando partida: " + _currentGameId);
    }

    private void OnDisable()
    {
        if (_botoIniciar != null)
        {
            _botoIniciar.clicked -= OnClickIniciar;
        }
    }
}