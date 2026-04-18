using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;


public class LoginController : MonoBehaviour
{
    // ─── REFERÈNCIES UI ───────────────────────────────────────────
    private VisualElement _panellLogin;
    private VisualElement _panellRegistre;

    // Camps login
    private TextField _campEmail;
    private TextField _campPassword;
    private Button _botoLogin;
    private Button _botoMostrarRegistre;
    private Label _textError;

    // Camps registre
    private TextField _campUsuari;
    private TextField _campEmailRegistre;
    private TextField _campPasswordRegistre;
    private Button _botoConfirmarRegistre;
    private Button _botoCancelarRegistre;
    private Label _textErrorRegistre;
    private Button _btnEnv;
    private Button _btnTema;

    // ─── INICIALITZACIÓ ───────────────────────────────────────────
    private void OnEnable()
    {
        // Obté l'arrel del document
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Querys dels panells
        _panellLogin = root.Q<VisualElement>("panell-login");
        _panellRegistre = root.Q<VisualElement>("panell-registre");

        // Querys login
        _campEmail = root.Q<TextField>("camp-email");
        _campPassword = root.Q<TextField>("camp-password");
        _botoLogin = root.Q<Button>("boto-login");
        _botoMostrarRegistre = root.Q<Button>("boto-mostrar-registre");
        _textError = root.Q<Label>("text-error");

        // Query para botones de configuración
        _btnTema = root.Q<Button>("btn-tema");
        _btnEnv = root.Q<Button>("btn-env");

        // Querys registre
        _campUsuari = root.Q<TextField>("camp-usuari");
        _campEmailRegistre = root.Q<TextField>("camp-email-registre");
        _campPasswordRegistre = root.Q<TextField>("camp-password-registre");
        _botoConfirmarRegistre = root.Q<Button>("boto-confirmar-registre");
        _botoCancelarRegistre = root.Q<Button>("boto-cancelar-registre");
        _textErrorRegistre = root.Q<Label>("text-error-registre");

        // Assigna els events
        _botoLogin.clicked += OnClickLogin;
        _botoMostrarRegistre.clicked += OnClickMostrarRegistre;
        _botoConfirmarRegistre.clicked += OnClickRegistre;
        _botoCancelarRegistre.clicked += OnClickTornarLogin;

        if (_btnTema != null)
        {
            _btnTema.clicked += OnClickTema;
            UpdateThemeButtonText();
            if (ThemeManager.Instance != null)
                ThemeManager.Instance.OnThemeChanged += OnThemeChanged;
        }

        if (_btnEnv != null)
        {
            _btnEnv.clicked += OnClickEnv;
            UpdateEnvButtonText();
        }

        // Permet fer login prement Enter
        _campPassword.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                OnClickLogin();
        });
    }

    private void OnDisable()
    {
        // Neteja els events per evitar memory leaks
        _botoLogin.clicked -= OnClickLogin;
        _botoMostrarRegistre.clicked -= OnClickMostrarRegistre;
        _botoConfirmarRegistre.clicked -= OnClickRegistre;
        _botoCancelarRegistre.clicked -= OnClickTornarLogin;

        if (_btnTema != null)
        {
            _btnTema.clicked -= OnClickTema;
            if (ThemeManager.Instance != null)
                ThemeManager.Instance.OnThemeChanged -= OnThemeChanged;
        }

        if (_btnEnv != null)
        {
            _btnEnv.clicked -= OnClickEnv;
        }
    }

    private void OnClickTema()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.ToggleTheme();
        }
    }

    private void OnClickEnv()
    {
        if (ServerConfig.Instance != null)
        {
            ServerConfig.Instance.ToggleEnvironment();
            UpdateEnvButtonText();
        }
        else {
            // Si el objeto no existe en escena (raro), lo creamos
            GameObject scObj = new GameObject("ServerConfig");
            scObj.AddComponent<ServerConfig>().ToggleEnvironment();
            UpdateEnvButtonText();
        }
    }

    private void OnThemeChanged(ThemeMode mode)
    {
        UpdateThemeButtonText();
    }

    private void UpdateThemeButtonText()
    {
        if (_btnTema != null && ThemeManager.Instance != null)
        {
            _btnTema.text = "MODE: " + (ThemeManager.Instance.currentTheme == ThemeMode.Day ? "DIA" : "NIT");
        }
    }

    private void UpdateEnvButtonText()
    {
        if (_btnEnv != null && ServerConfig.Instance != null)
        {
            _btnEnv.text = "ENV: " + (ServerConfig.Instance.CurrentEnvironment == ServerEnvironment.Local ? "LOCAL" : "SERVER");
            // Cambiar color para distinguir visualmente
            if (ServerConfig.Instance.CurrentEnvironment == ServerEnvironment.Remote)
                _btnEnv.style.color = new StyleColor(new Color(1f, 0.8f, 0.2f)); // Amarillo/Dorado
            else
                _btnEnv.style.color = new StyleColor(Color.white);
        }
    }

    // ─── LOGIN ────────────────────────────────────────────────────
    private void OnClickLogin()
    {
        string email = _campEmail.value.Trim();
        string password = _campPassword.value;

        // Validació al client
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            MostrarError(_textError, "Omple tots els camps.");
            return;
        }

        // Desactiva el botó mentre espera
        SetBotoActiu(_botoLogin, false);
        MostrarError(_textError, "Connectant...", false);

        AuthManager.Instance.FerLogin(email, password, (exit, missatge) =>
        {
            SetBotoActiu(_botoLogin, true);

            if (exit)
            {
                SceneManager.LoadScene("MenuPrincipal");
            }
            else
            {
                MostrarError(_textError, missatge);
            }
        });
    }

    // ─── NAVEGACIÓ ENTRE PANELLS ──────────────────────────────────
    private void OnClickMostrarRegistre()
    {
        _panellLogin.AddToClassList("panell-ocult");
        _panellRegistre.RemoveFromClassList("panell-ocult");
        MostrarError(_textErrorRegistre, "");
    }

    private void OnClickTornarLogin()
    {
        _panellRegistre.AddToClassList("panell-ocult");
        _panellLogin.RemoveFromClassList("panell-ocult");
        MostrarError(_textError, "");
    }

    // ─── REGISTRE ─────────────────────────────────────────────────
    private void OnClickRegistre()
    {
        string usuari = _campUsuari.value.Trim();
        string email = _campEmailRegistre.value.Trim();
        string password = _campPasswordRegistre.value;

        // Validació al client
        if (string.IsNullOrEmpty(usuari) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password))
        {
            MostrarError(_textErrorRegistre, "Omple tots els camps.");
            return;
        }

        if (password.Length < 6)
        {
            MostrarError(_textErrorRegistre, "La contrasenya ha de tenir mínim 6 caràcters.");
            return;
        }

        SetBotoActiu(_botoConfirmarRegistre, false);
        MostrarError(_textErrorRegistre, "Registrant...", false);

        AuthManager.Instance.FerRegistre(usuari, email, password, (exit, missatge) =>
        {
            SetBotoActiu(_botoConfirmarRegistre, true);

            if (exit)
            {
                // Registre correcte → torna al login automàticament
                MostrarError(_textErrorRegistre, missatge, false);
                // Omple l'email al formulari de login per comoditat
                _campEmail.value = email;
                Invoke(nameof(OnClickTornarLogin), 1.5f);
            }
            else
            {
                MostrarError(_textErrorRegistre, missatge);
            }
        });
    }

    // ─── UTILITATS ────────────────────────────────────────────────
    private void MostrarError(Label label, string missatge, bool esError = true)
    {
        label.text = missatge;
        // Canvia el color segons si és error o info
        label.style.color = esError
            ? new StyleColor(new Color(1f, 0.31f, 0.31f))   // vermell
            : new StyleColor(new Color(0.63f, 0.63f, 0.78f)); // gris clar
    }

    private void SetBotoActiu(Button boto, bool actiu)
    {
        boto.SetEnabled(actiu);
    }
}