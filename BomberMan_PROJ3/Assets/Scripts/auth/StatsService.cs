using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class StatsService : MonoBehaviour 
{
    public static StatsService Instance;
    private string _token;

    private void Awake() 
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SetToken(string token) 
    {
        _token = token;
    }

    public void ReportGameResult(string gameId, string winnerId, List<PlayerResultData> players)
    {
        StartCoroutine(PostGameResult(gameId, winnerId, players));
    }

    private IEnumerator PostGameResult(string gameId, string winnerId, List<PlayerResultData> players)
    {
        string url = ServerConfig.Instance.GetBaseUrl("/estadistiques/resultat");
        
     
        string playersJson = "[";
        for (int i = 0; i < players.Count; i++)
        {
            playersJson += "{\"playerId\":\"" + players[i].playerId + "\",\"score\":" + players[i].score + "}";
            if (i < players.Count - 1) playersJson += ",";
        }
        playersJson += "]";

        string jsonBody = "{\"gameId\":\"" + gameId + "\",\"winnerId\":\"" + winnerId + "\",\"players\":" + playersJson + "}";
        
        Debug.Log("[StatsService] Enviant resultats: " + jsonBody);

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // El token se suele guardar en AuthManager
        string token = _token;
        if (string.IsNullOrEmpty(token) && AuthManager.Instance != null) {
            token = AuthManager.Instance.TokenJWT;
        }

        if (string.IsNullOrEmpty(token)) {
            Debug.LogError("[StatsService]  No es poden enviar resultats: No hi ha Token!");
            yield break;
        }

        request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            Debug.Log("[StatsService] Resultats guardats amb èxit");
        }
        request.Dispose();
    }
}

[System.Serializable]
public class PlayerResultData
{
    public string playerId;
    public int score;
}
