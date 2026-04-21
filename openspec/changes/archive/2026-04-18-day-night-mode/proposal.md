## Why

Adds a visual "Day/Night Mode" to the game, enhancing the retro aesthetic and providing player choice for atmospheric gameplay. This feature allows players to customize their visual experience and adds a layer of polish common in modern-retro titles.

## What Changes

- **UI Toggle**: A new UI element (Toggle or Button) in the `MainMenu.uxml` to switch between Day and Night modes.
- **Visual Aesthetic**: 
    - **Day Mode**: Standard bright lighting (White light, Intensity 1.0).
    - **Night Mode**: Darker, atmospheric tinting (Blueish tint `#1a2639`, Intensity 0.4).
- **Persistence**: Player selection is saved automatically using `PlayerPrefs` and re-applied on game launch.
- **Scene Integration**: Automatic detection and adjustment of the `Universal (Global) Light 2D` in the game scene.

## Capabilities

### New Capabilities
- `theme-persistence`: Logic for saving and loading player theme preferences.
- `ambient-lighting-control`: Logic for adjusting scene lighting based on the active theme.
- `theme-switcher-ui`: UI components and styles for toggling the theme in menus.

### Modified Capabilities
- None (Initial implementation of theme system).

## Impact

- **UI**: `MainMenu.uxml` and `MainMenuStyles.uss` will be updated with the new toggle.
- **Scripts**: New `ThemeManager.cs` Singleton.
- **Scenes**: GameScene `Global Light 2D` will be targeted by the new manager.
