using UnityEngine;
using UnityEngine.UIElements;

public class MenuPrincipalController : MonoBehaviour
{
    private Button _btnCrear;
    private Button _btnLLM;
    private Button _btnUnirse;
    private TextField _inputCodigo;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _btnCrear   = root.Q<Button>("btn-crear");
        _btnLLM     = root.Q<Button>("btn-llm");
        _btnUnirse  = root.Q<Button>("btn-unirse");
        _inputCodigo = root.Q<TextField>("input-codigo");

        _btnCrear.clicked  += OnClickCrear;
        _btnLLM.clicked    += OnClickLLM;
        _btnUnirse.clicked += OnClickUnirse;
    }

    private void OnDisable()
    {
        _btnCrear.clicked  -= OnClickCrear;
        _btnLLM.clicked    -= OnClickLLM;
        _btnUnirse.clicked -= OnClickUnirse;
    }

    private void OnClickCrear()
{
    Debug.Log("Creando sala...");
    if (GameService.Instance != null) 
    {
        StartCoroutine(GameService.Instance.CreateGameRoom((responseJson) => {
            GameResponse game = JsonUtility.FromJson<GameResponse>(responseJson);
            string gameId = game._id;
            
            Debug.Log("✅ SALA CREADA - COPIA ESTE ID: " + gameId);
            AuthManager.Instance.PlayerIndex = 0; // Host es siempre Player 0
            PlayerPrefs.SetString("current_game_id", gameId);
        }));
    }
}

    private void OnClickLLM()
    {
        Debug.Log("Jugant contra LLM / Entrenament...");
        // Carreguem la escena de prova/entrenament
        UnityEngine.SceneManagement.SceneManager.LoadScene("TrainingScene");
    }

   private void OnClickUnirse()
{
    string code = _inputCodigo.value.Trim();
    if (string.IsNullOrEmpty(code))
    {
        Debug.LogWarning("Introduce el código de 6 dígitos");
        return;
    }
    
    Debug.Log("Buscando sala con código: " + code);
    _btnUnirse.SetEnabled(false);
    
    StartCoroutine(GameService.Instance.GetGameByCode(code, 
        onSuccess: (gameJson) => {
            GameResponse game = JsonUtility.FromJson<GameResponse>(gameJson);
            string gameId = game._id;
            
            StartCoroutine(GameService.Instance.JoinGameRoom(gameId, 
                onSuccess: (joinJson) => {
                    Debug.Log("✅ Unido a la sala");
                    AuthManager.Instance.PlayerIndex = 1; // Joiner es Player 1 (índice real)
                    MenuSala menuSala = FindObjectOfType<MenuSala>();
                    if (menuSala != null) {
                        menuSala.JoinExistingGame(gameId, code);
                    }
                },
                onError: (err) => {
                    Debug.LogError("❌ Error: " + err);
                    _btnUnirse.SetEnabled(true);
                }
            ));
        },
        onError: (err) => {
            Debug.LogError("❌ Código no encontrado: " + err);
            _btnUnirse.SetEnabled(true);
        }
    ));
}
}