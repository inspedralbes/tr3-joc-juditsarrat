## ADDED Requirements

### Requirement: Global lighting follows the active theme
The system SHALL adjust the `Universal Light 2D` (Global Light) parameters in the scene to match the active theme.

#### Scenario: Apply Day theme lighting
- **WHEN** Day mode is the active theme
- **THEN** Global Light 2D intensity is set to 1.0 and color is set to White (#FFFFFF)

#### Scenario: Apply Night theme lighting
- **WHEN** Night mode is the active theme
- **THEN** Global Light 2D intensity is set to 0.4 and color is set to Deep Blue (#1A2639)

### Requirement: Fallback for missing Global Light
The system SHALL attempt a fallback color adjustment if the specific light component is missing from the scene.

#### Scenario: Camera background fallback
- **WHEN** a theme is applied but no Global Light 2D is found in the scene
- **THEN** the Main Camera's background color is adjusted to match the theme (e.g., Black/Dark Blue for Night)
