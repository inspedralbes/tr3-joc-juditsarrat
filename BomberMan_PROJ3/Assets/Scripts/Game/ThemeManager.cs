using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System;

public enum ThemeMode
{
    Day = 0,
    Night = 1
}

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [Header("Theme Settings")]
    public ThemeMode currentTheme = ThemeMode.Day;
    
    [Header("Lighting Settings")]
    public Color nightColor = new Color(0.1f, 0.15f, 0.22f); // #1a2639 approximate
    public float nightIntensity = 0.4f;
    public Color dayColor = new Color(1f, 0.95f, 0.5f); // Tinte amarillento claro
    public float dayIntensity = 1.0f;

    public event Action<ThemeMode> OnThemeChanged;

    private const string PREF_KEY = "UserThemeMode";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("ThemeManager");
            go.AddComponent<ThemeManager>();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTheme();
            ApplyThemeToCurrentScene(); // Aplicar nada más cargar
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ToggleTheme()
    {
        currentTheme = (currentTheme == ThemeMode.Day) ? ThemeMode.Night : ThemeMode.Day;
        SaveTheme();
        ApplyThemeToCurrentScene();
        OnThemeChanged?.Invoke(currentTheme);
    }

    private void SaveTheme()
    {
        PlayerPrefs.SetInt(PREF_KEY, (int)currentTheme);
        PlayerPrefs.Save();
    }

    private void LoadTheme()
    {
        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            currentTheme = (ThemeMode)PlayerPrefs.GetInt(PREF_KEY);
        }
        else
        {
            currentTheme = ThemeMode.Day;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyThemeToCurrentScene();
    }

    public void ApplyThemeToCurrentScene()
    {
        // 1. Aplicar a la Iluminación (2D Lights)
        Light2D globalLight = FindGlobalLight();
        if (globalLight != null)
        {
            globalLight.color = (currentTheme == ThemeMode.Night) ? nightColor : dayColor;
            globalLight.intensity = (currentTheme == ThemeMode.Night) ? nightIntensity : dayIntensity;
            Debug.Log($"[ThemeManager] Applied {currentTheme} theme to Global Light 2D.");
        }
        else
        {
            ApplyCameraFallback();
        }

        // 2. Aplicar a la Interfaz (UI Toolkit)
        UIDocument[] docs = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        foreach (var doc in docs)
        {
            var root = doc.rootVisualElement;
            if (root != null)
            {
                // Limpiar clases previas
                root.RemoveFromClassList("theme-day");
                root.RemoveFromClassList("theme-night");
                
                // Aplicar nueva clase
                string classToAdd = (currentTheme == ThemeMode.Day) ? "theme-day" : "theme-night";
                root.AddToClassList(classToAdd);
                
                Debug.Log($"[ThemeManager] Applied {classToAdd} to UI Root: {doc.gameObject.name}");
            }
        }
    }

    private Light2D FindGlobalLight()
    {
        Light2D[] lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            if (light.lightType == Light2D.LightType.Global)
            {
                return light;
            }
        }
        return null;
    }

    private void ApplyCameraFallback()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (currentTheme == ThemeMode.Night)
            {
                mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            }
            else
            {
                mainCam.backgroundColor = new Color(0.19f, 0.19f, 0.19f); // Default dark grey
            }
            Debug.Log($"[ThemeManager] Applied {currentTheme} theme fallback to Camera.");
        }
    }
}
