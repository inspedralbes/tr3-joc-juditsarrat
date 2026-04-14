using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameService : MonoBehaviour {
    public static GameService Instance;
    private string _token;
    
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
        Debug.Log("[GameService] Token establecido");
    }

    public IEnumerator GetGameStatus(string gameId, System.Action<string> onSuccess, System.Action<string> onError = null) {
        string url = "http://127.0.0.1:8080/joc/games/" + gameId;
        Debug.Log("[GameService] Obteniendo sala: " + gameId);

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + _token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            Debug.Log("[GameService] ✅ Sala encontrada");
            onSuccess?.Invoke(request.downloadHandler.text);
        } else {
            onError?.Invoke("Sala no encontrada");
        }
        request.Dispose();
    }

    public IEnumerator CreateGameRoom(System.Action<string> onSuccess, System.Action<string> onError = null) {
        string url = "http://127.0.0.1:8080/joc/games";

        if (string.IsNullOrEmpty(_token)) {
            onError?.Invoke("Token no establecido");
            yield break;
        }

        string jsonBody = "{\"hostId\":\"" + AuthManager.Instance.JugadorActual.id + "\",\"config\":{\"maxPlayers\":2}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + _token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            Debug.Log("[GameService] ✅ Sala creada");
            onSuccess?.Invoke(request.downloadHandler.text);
        } else {
            onError?.Invoke("Error creando sala");
        }
        request.Dispose();
    }

    public IEnumerator GetGameByCode(string code, System.Action<string> onSuccess, System.Action<string> onError = null) {
        string url = "http://127.0.0.1:8080/joc/games/code/" + code;
        Debug.Log("[GameService] Buscando: " + code);

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + _token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            onSuccess?.Invoke(request.downloadHandler.text);
        } else {
            onError?.Invoke("Código no encontrado");
        }
        request.Dispose();
    }

    public IEnumerator JoinGameRoom(string gameId, System.Action<string> onSuccess, System.Action<string> onError = null) {
        string url = "http://127.0.0.1:8080/joc/games/" + gameId + "/join";
        string playerId = AuthManager.Instance.JugadorActual.id;
        string jsonBody = "{\"playerId\":\"" + playerId + "\"}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + _token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            onSuccess?.Invoke(request.downloadHandler.text);
        } else {
            onError?.Invoke("Error uniéndose a sala");
        }
        request.Dispose();
    }
}