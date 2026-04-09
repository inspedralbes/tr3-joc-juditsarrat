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
    Debug.Log("Iniciant petició de creació...");
    
    // CAMBIO AQUÍ: De GameClient a GameService
    if (GameService.Instance != null) 
    {
        StartCoroutine(GameService.Instance.CreateGameRoom((gameId) => {
            Debug.Log("Sala creada amb èxit! ID: " + gameId);
        }));
    }
    else 
    {
        Debug.LogError("Error: No s'ha trobat la instancia de GameService en l'escena.");
    }
}    private void OnClickLLM()
    {
        Debug.Log("Jugant contra LLM...");
        // Aquí navegarem a l'escena del joc contra IA
    }

    private void OnClickUnirse()
    {
        string codigo = _inputCodigo.value.Trim();

        if (string.IsNullOrEmpty(codigo))
        {
            Debug.LogWarning("Cal introduir un codi de sala.");
            return;
        }

        Debug.Log("Unint-se a la sala: " + codigo);
        // Aquí cridarem el WebSocket per unir-se a la sala
    }
}