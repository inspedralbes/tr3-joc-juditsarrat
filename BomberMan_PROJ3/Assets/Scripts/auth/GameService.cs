// En un nuevo script llamado GameService.cs
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameService : MonoBehaviour {
    public static GameService Instance;
    private string _token; // Aquí guardas el JWT que recibiste en el Login
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SetToken(string token) {
        _token = token;
        Debug.Log("[GameService] Token establert correctament");
    }


public IEnumerator GetGameStatus(string gameId, System.Action<string> onSuccess, System.Action<string> onError = null) {
    string url = "http://localhost:8080/joc/games/" + gameId;

    Debug.Log("[GameService] Obteniendo estado de sala: " + gameId);

    UnityWebRequest request = UnityWebRequest.Get(url);
    request.SetRequestHeader("Authorization", "Bearer " + _token);

    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success) {
        Debug.Log("[GameService] ✅ Estado obtenido");
        onSuccess?.Invoke(request.downloadHandler.text);
    } else {
        string errorMsg = "Error HTTP " + request.responseCode + ": " + request.error;
        Debug.LogError("[GameService] ❌ " + errorMsg);
        onError?.Invoke(errorMsg);
    }

    request.Dispose();
}

    public IEnumerator CreateGameRoom(System.Action<string> onSuccess, System.Action<string> onError = null) {
        string url = "http://localhost:8080/joc/games";

        Debug.Log("[GameService] Intentant crear sala a: " + url);

        if (string.IsNullOrEmpty(_token)) {
            string errorMsg = "Token no establecido. Debes hacer login primero.";
            Debug.LogError("[GameService] " + errorMsg);
            onError?.Invoke(errorMsg);
            yield break;
        }


    

        // Crear JSON
        string jsonBody = "{\"hostId\":\"" + AuthManager.Instance.JugadorActual.id + "\",\"config\":{\"maxPlayers\":2}}";
        Debug.Log("[GameService] JSON enviado: " + jsonBody);

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + _token);

        Debug.Log("[GameService] Enviando petición...");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            Debug.Log("[GameService] ✅ Respuesta exitosa: " + request.downloadHandler.text);
            onSuccess?.Invoke(request.downloadHandler.text);
        } else {
            string errorMsg = "Error HTTP " + request.responseCode + ": " + request.error + "\nRespuesta: " + request.downloadHandler.text;
            Debug.LogError("[GameService] ❌ " + errorMsg);
            onError?.Invoke(errorMsg);
        }

        request.Dispose();
    }
}   