## Context

The game currently lacks a unified visual theme system. It utilizes Unity's Universal Render Pipeline (URP) for rendering, but all scenes default to standard environment lighting. The UI is built with UI Toolkit, and there is no mechanism for saving user visual preferences.

## Goals / Non-Goals

**Goals:**
- Implement a persistent "Day/Night" toggle in the Main Menu.
- Automatically apply the selected theme (colors and light intensity) when the game or a match starts.
- Ensure the system is robust against missing light components in experimental scenes.

**Non-Goals:**
- Real-time "time of day" cycling (the change is static once selected).
- Changing gameplay mechanics (e.g., visibility radius for players) in this initial version.
- Per-map theme overrides.

## Decisions

### 1. Persistent `ThemeManager` Singleton
We will implement a `ThemeManager` script using the Singleton pattern (`DontDestroyOnLoad`). 
- **Rationale**: The theme needs to persist across scene transitions (Login -> Menu -> Game) and be the central authority for the current state.
- **Alternatives**: Passing data via `SceneManager` arguments (messy) or storing it in `AuthManager` (poor separation of concerns).

### 2. UI Toolkit Integration
The toggle will be added to `MainMenu.uxml` using a `Button` that cycles states or a `Toggle` element.
- **Rationale**: Consistency with the existing project's UI architecture.

### 3. Light 2D as Primary Target
The system will primarily target the `Universal Light 2D` (Global) component.
- **Rationale**: This is the standard and most performant way to tint a 2D world in URP.
- **Fallback**: If no light is found, the system will attempt to tint `Camera.backgroundColor` to preserve the "vibe" without breaking the scene.

### 4. Storage via `PlayerPrefs`
Theme selection will be stored under the key `UserThemeMode`.
- **Rationale**: `PlayerPrefs` is ideal for simple user preferences like this, requiring zero infrastructure.

## Risks / Trade-offs

- **[Risk] Missing Global Light** → [Mitigation] Implement a fallback to `Camera` coloring and add detailed logging if the light isn't found.
- **[Risk] UI State Out of Sync** → [Mitigation] `ThemeManager` will expose an event (e.g., `OnThemeChanged`) that the Main Menu controller can subscribe to for updating its text/icon.
