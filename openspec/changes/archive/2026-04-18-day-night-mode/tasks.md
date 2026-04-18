## 1. Core Logic & Persistence

- [x] 1.1 Create `ThemeManager.cs` script as a persistent Singleton (`DontDestroyOnLoad`).
- [x] 1.2 Implement `ThemeMode` enum (Day, Night).
- [x] 1.3 Implement `PlayerPrefs` logic to save and load the `UserThemeMode` key.
- [x] 1.4 Implement an event or callback system to notify other components when the theme changes.

## 2. UI Integration

- [x] 2.1 Add a new Button or Toggle to `MainMenu.uxml` within the main menu section.
- [x] 2.2 Update `MainMenuStyles.uss` to style the new UI element in the NES/Bomberman style.
- [x] 2.3 Update `MenuPrincipalController.cs` to hook up the UI event to the `ThemeManager`'s toggle logic.
- [x] 2.4 Ensure the UI button text/label updates dynamically to display "DIA" or "NIT" based on current state.

## 3. Environmental Lighting Control

- [x] 3.1 Implement target detection in `ThemeManager` to find the `Universal Light 2D` (Global) in the scene.
- [x] 3.2 Implement lighting parameter adjustment (Day: 1.0/White, Night: 0.4/Deep Blue).
- [x] 3.3 Implement fallback logic to adjust the `Camera.backgroundColor` if no light component is found.
- [x] 3.4 Ensure the theme is correctly applied immediately upon scene loading.
