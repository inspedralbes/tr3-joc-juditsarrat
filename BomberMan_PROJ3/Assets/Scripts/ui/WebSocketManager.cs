using UnityEngine;
using NativeWebSocket;
using System;
using System.Collections;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance { get; private set; }
    private WebSocket _webSocket;
    private string _gameId;
    private string _playerId;
    
    public event Action<int> OnPlayerCountChanged;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public event Action<string> OnMessageReceived;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static WebSocketManager GetOrCreate()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("WebSocketManager");
            Instance = obj.AddComponent<WebSocketManager>();
        }
        return Instance;
    }
    


    public void ConnectToGame(string gameId)
    {
        if (_webSocket != null && (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.Connecting))
        {
            Debug.LogWarning("[WebSocket] Ya hay una conexión activa");
            return;
        }
        
        _gameId = gameId;
        _playerId = AuthManager.Instance.JugadorActual.id;
        
        string wsUrl = "ws://localhost:8080/joc/?gameId=" + _gameId + "&playerId=" + _playerId;
        Debug.Log("[WebSocket] Conectando a (Gateway): " + wsUrl);
        
        StartCoroutine(Connect(wsUrl));
    }
    
    private IEnumerator Connect(string url)
    {
        _webSocket = new WebSocket(url);
        
        _webSocket.OnOpen += HandleWebSocketOpen;
        _webSocket.OnMessage += HandleWebSocketMessage;
        _webSocket.OnError += HandleWebSocketError;
        
        yield return _webSocket.Connect();
        
        yield return new WaitForSeconds(0.5f);
        
        if (_webSocket.State == WebSocketState.Open)
        {
            Debug.Log("[WebSocket] ✅ Conexión establecida");
            OnConnected?.Invoke();
        }
        else
        {
            Debug.LogError("[WebSocket] Estado: " + _webSocket.State);
        }
    }

        private void HandleWebSocketOpen()
{
    Debug.Log("[WebSocket] ✅ Conectado");
    
    string joinMessage = "{\"type\":\"join-game\",\"gameId\":\"" + _gameId + "\",\"playerId\":\"" + _playerId + "\"}";
    _webSocket.SendText(joinMessage);
} 
    
    private void HandleWebSocketMessage(byte[] bytes)
{
    string message = System.Text.Encoding.UTF8.GetString(bytes);
    Debug.Log("[WebSocket] 📨 Mensaje: " + message);
    
    OnMessageReceived?.Invoke(message);  // ← AGREGA ESTA LÍNEA
    
    try
    {
        ProcessMessage(message);
    }
    catch (System.Exception e)
    {
        Debug.LogWarning("[WebSocket] Error procesando: " + e.Message);
    }
}
       
    
    private void ProcessMessage(string jsonMessage)
    {
        // Extraer totalPlayers si existe
        if (jsonMessage.Contains("\"totalPlayers\""))
        {
            int playerCount = ExtractPlayerCount(jsonMessage);
            if (playerCount > 0)
            {
                Debug.Log("[WebSocket] 👥 Jugadores: " + playerCount);
                OnPlayerCountChanged?.Invoke(playerCount);
            }
        }
        
        if (jsonMessage.Contains("\"type\":\"player-joined\""))
        {
            Debug.Log("[WebSocket] Un jugador se unió");
        }
        else if (jsonMessage.Contains("\"type\":\"game-started\""))
        {
            Debug.Log("[WebSocket] El juego comenzó");
        }
        else if (jsonMessage.Contains("\"type\":\"player-moved\""))
        {
            Debug.Log("[WebSocket] Un jugador se movió");
        }
        else if (jsonMessage.Contains("\"type\":\"bomb-placed\""))
        {
            Debug.Log("[WebSocket] Se colocó una bomba");
        }
        else if (jsonMessage.Contains("\"type\":\"player-disconnected\""))
        {
            Debug.Log("[WebSocket] Un jugador se desconectó");
        }
    }
    
    private int ExtractPlayerCount(string json)
    {
        try
        {
            int start = json.IndexOf("\"totalPlayers\"") + 15;
            int end = json.IndexOf(",", start);
            if (end == -1) end = json.IndexOf("}", start);
            
            string countStr = json.Substring(start, end - start).Trim();
            if (int.TryParse(countStr, out int result))
            {
                return result;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[WebSocket] Error extrayendo contador: " + e.Message);
        }
        return 0;
    }
    
    private void HandleWebSocketError(string errorMsg)
    {
        Debug.LogError("[WebSocket] ❌ Error: " + errorMsg);
        OnDisconnected?.Invoke();
        OnError?.Invoke(errorMsg);
    }
    
    public void SendMessage(string messageType, string data)
    {
        if (_webSocket == null)
        {
            Debug.LogError("[WebSocket] WebSocket es null");
            return;
        }
        
        if (_webSocket.State != WebSocketState.Open)
        {
            Debug.LogError("[WebSocket] WebSocket no está abierto. Estado: " + _webSocket.State);
            return;
        }
        
        string json = "{\"type\":\"" + messageType + "\",\"data\":" + data + "}";
        Debug.Log("[WebSocket] 📤 Enviando: " + json);
        _webSocket.SendText(json);
    }
    
    private void Update()
    {
        if (_webSocket != null)
        {
            _webSocket.DispatchMessageQueue();
        }
    }
    
    public void Disconnect()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            Debug.Log("[WebSocket] Desconectando...");
            _webSocket.Close();
        }
    }
    
    private void OnDestroy()
    {
        Disconnect();
    }
}