using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LoginResponse
{
    public string token;
    public UserData user;
}

[Serializable]
public class RegisterResponse
{
    public string missatge;
    public UserData user;
}

[Serializable]
public class UserData
{
    public string id;
    public string username;
    public string email;
}

[Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[Serializable]
public class RegisterRequest
{
    public string username;
    public string email;
    public string password;
}

public class AuthManager : MonoBehaviour
{
    private string URL_BASE => ServerConfig.Instance != null ? ServerConfig.Instance.GetBaseUrl("/auth") : "http://127.0.0.1:8080/auth";

    // ─── SINGLETON ────────────────────────────────────────────────
    public static AuthManager Instance { get; private set; }

    public string TokenJWT { get; private set; }
    public UserData JugadorActual { get; private set; }
    public int PlayerIndex { get; set; } // 0 = Player 1 (Host), 1 = Player 2
    public bool EstaAutenticat => !string.IsNullOrEmpty(TokenJWT);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── LOGIN ────────────────────────────────────────────────────
    public void FerLogin(string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(CoroutinaLogin(email, password, callback));
    }

   private IEnumerator CoroutinaLogin(string email, string password, Action<bool, string> callback)
{
    var peticio = new LoginRequest { email = email, password = password };
    string json = JsonUtility.ToJson(peticio);
    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

    using var www = new UnityWebRequest(URL_BASE + "/login", "POST");
    www.uploadHandler = new UploadHandlerRaw(bytes);
    www.downloadHandler = new DownloadHandlerBuffer();
    www.SetRequestHeader("Content-Type", "application/json");

    yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string respuestaCompleta = www.downloadHandler.text;
            Debug.Log("[AuthManager] ✅ Respuesta completa del servidor:");
            Debug.Log(respuestaCompleta);
            
            var resposta = JsonUtility.FromJson<LoginResponse>(respuestaCompleta);
            Debug.Log("[AuthManager] Token: " + resposta.token);
            Debug.Log("[AuthManager] User object: " + (resposta.user != null ? "EXISTS" : "NULL"));
            
            if (resposta.user != null) {
                Debug.Log("[AuthManager] User.id: " + resposta.user.id);
                Debug.Log("[AuthManager] User.username: " + resposta.user.username);
                Debug.Log("[AuthManager] User.email: " + resposta.user.email);
            }
            
            TokenJWT = resposta.token;
            JugadorActual = resposta.user;
            // ✅ MILLOR VALIDACIÓ
            Debug.Log("[AuthManager] Buscant GameService...");
            GameService gameService = FindFirstObjectByType<GameService>();
            if (gameService != null)
            {
                gameService.SetToken(TokenJWT);
                Debug.Log("[AuthManager] Token passat a GameService ✅");
            }
            else
            {
                Debug.LogWarning("[AuthManager] GameService no trobat a la scena. Creant-lo...");
                GameObject gsObj = new GameObject("GameService");
                gameService = gsObj.AddComponent<GameService>();
                gameService.SetToken(TokenJWT);
            }

            // ✅ INICIALITZAR TAMBÉ STATS SERVICE
            StatsService statsService = FindFirstObjectByType<StatsService>();
            if (statsService == null)
            {
                GameObject ssObj = new GameObject("StatsService");
                statsService = ssObj.AddComponent<StatsService>();
            }
            statsService.SetToken(TokenJWT);
            Debug.Log("[AuthManager] StatsService a punt ✅");
            
            callback(true, "Login correcte");
        }
        else
        {
            // ❌ GESTIÓ D'ERROR AFEGIDA PER EVITAR QUE EL BUILD ES QUEDI PITJAT
            string error = ExtreureErrorServidor(www.downloadHandler.text);
            Debug.LogError("[AuthManager] ❌ Error en el login: " + error);
            callback(false, error);
        }
    }

    // ─── REGISTRE ─────────────────────────────────────────────────
    public void FerRegistre(string username, string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(CoroutinaRegistre(username, email, password, callback));
    }

    private IEnumerator CoroutinaRegistre(string username, string email, string password, Action<bool, string> callback)
    {
        var peticio = new RegisterRequest { username = username, email = email, password = password };
        string json = JsonUtility.ToJson(peticio);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

        using var www = new UnityWebRequest(URL_BASE + "/register", "POST");
        www.uploadHandler = new UploadHandlerRaw(bytes);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            callback(true, "Registre correcte! Ara pots entrar.");
        }
        else
        {
            string error = ExtreureErrorServidor(www.downloadHandler.text);
            callback(false, error);
        }
    }

    // ─── TANCAR SESSIÓ ────────────────────────────────────────────
    public void TancarSessio()
    {
        TokenJWT = null;
        JugadorActual = null;
    }

    // ─── UTILITAT ─────────────────────────────────────────────────
    private string ExtreureErrorServidor(string jsonResposta)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonResposta)) 
                return "No s'ha rebut resposta del servidor";

            var err = JsonUtility.FromJson<ErrorResponse>(jsonResposta);
            return string.IsNullOrEmpty(err?.error) ? "Error desconegut del servidor" : err.error;
        }
        catch
        {
            // Pot ser que la resposta no sigui JSON (ex: un error 504 de Nginx)
            return "Error de connexió amb el servidor";
        }
    }

    [Serializable]
    private class ErrorResponse { public string error; }
}