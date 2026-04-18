using UnityEngine;

public enum ServerEnvironment
{
    Local,
    Remote
}

public class ServerConfig : MonoBehaviour
{
    public static ServerConfig Instance { get; private set; }

    [Header("Configuration")]
    public string remoteIp = "135.181.204.198";
    public string port = "8080";
    
    [SerializeField] private ServerEnvironment currentEnvironment = ServerEnvironment.Local;

    public ServerEnvironment CurrentEnvironment => currentEnvironment;

    private const string PREF_KEY = "ServerEnvironment";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadEnvironment();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleEnvironment()
    {
        currentEnvironment = (currentEnvironment == ServerEnvironment.Local) 
            ? ServerEnvironment.Remote 
            : ServerEnvironment.Local;
        
        PlayerPrefs.SetInt(PREF_KEY, (int)currentEnvironment);
        PlayerPrefs.Save();
        
        Debug.Log($"[ServerConfig] Entorn canviat a: {currentEnvironment} ({GetBaseUrl("")})");
    }

    private void LoadEnvironment()
    {
        currentEnvironment = (ServerEnvironment)PlayerPrefs.GetInt(PREF_KEY, (int)ServerEnvironment.Local);
    }

    public string GetBaseUrl(string servicePath)
    {
        string host = (currentEnvironment == ServerEnvironment.Local) ? "127.0.0.1" : remoteIp;
        return $"http://{host}:{port}{servicePath}";
    }

    public string GetWsUrl(string servicePath)
    {
        string host = (currentEnvironment == ServerEnvironment.Local) ? "127.0.0.1" : remoteIp;
        return $"ws://{host}:{port}{servicePath}";
    }
}
