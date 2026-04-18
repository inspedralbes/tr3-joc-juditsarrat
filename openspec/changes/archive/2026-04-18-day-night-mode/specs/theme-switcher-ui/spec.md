## ADDED Requirements

### Requirement: Toggle UI reflects current theme
The theme toggle button (or label) in the Main Menu SHALL dynamically update its text to inform the user of the active theme.

#### Scenario: Display current mode
- **WHEN** the Main Menu is opened
- **THEN** the theme toggle button displays the name of the currently active mode (e.g., "MODE: DIA" or "MODE: NIT")

#### Scenario: Update on toggle
- **WHEN** the user interacts with the theme toggle
- **THEN** the button text updates immediately to the new state
