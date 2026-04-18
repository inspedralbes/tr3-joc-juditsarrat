## ADDED Requirements

### Requirement: User theme selection is stored
The system SHALL save the user's theme selection (Day/Night) automatically using `PlayerPrefs`.

#### Scenario: Save on theme change
- **WHEN** the user toggles the theme in the Main Menu
- **THEN** the selection is saved in `PlayerPrefs` under the key `UserThemeMode`

#### Scenario: Load on application start
- **WHEN** the game is launched
- **THEN** the system loads the previously saved theme from `PlayerPrefs` (defaulting to Day if none exists)
