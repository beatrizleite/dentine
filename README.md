# Dentine

[![GitHub Pre-Release](https://img.shields.io/github/v/release/beatrizleite/dentine?include_prereleases&label=pre-release)](https://github.com/beatrizleite/dentine/releases)

Dentine is a mobile serious game that raises oral health awareness among children aged 3 to 5. It was designed for pre-literate children: the gameplay relies on images, animation, colour-coded feedback and recorded narration in European Portuguese rather than written instructions.

The game was developed as part of a Master's dissertation in Computer Engineering at Universidade Fernando Pessoa (Porto, Portugal), in collaboration with the Faculty of Health Sciences.

## Gameplay

The game is a linear sequence of three levels, each followed by a five-question quiz.

| Level | Topic | Interaction |
| --- | --- | --- |
| 1. Nutritional awareness | Foods that are good or bad for the teeth | Drag 20 foods into the happy or sad tooth basket |
| 2. Oral hygiene | Brushing and rinsing | Scrub the teeth and tongue with the toothbrush, then shake the phone to rinse |
| 3. Dental office | Getting familiar with dental instruments | Memory matching game with narrated instrument cards |

After each level, a quiz with image-based answers and a narration button reinforces the topic. Completing all three levels unlocks a diploma.

## Accessibility

- Every level starts with a narrated introduction and an animated demonstration of the gesture.
- Idle hints repeat the demonstration if the child stops interacting.
- Correct and incorrect actions use the same signals across levels: green for success, a shaking red for errors.
- Quiz questions and instrument descriptions are narrated by a human voice.
- Large touch targets and forgiving drop zones accommodate developing motor skills.

## Privacy

Dentine works fully offline. It makes no network requests, collects no personal data and does not request internet permission. Progress is stored locally on the device.

## Installation

1. Download the latest APK from the [Releases](https://github.com/beatrizleite/dentine/releases) page.
2. On your Android device, allow installation from unknown sources if prompted.
3. Open the APK to install.

Requirements: Android 7.0 (API 24) or later, a 64-bit (ARM64) device and an accelerometer, which is needed for the rinsing step in Level 2. The game runs in landscape orientation.

## Building from source

1. Install Unity 6 (6000.2.10f1) with the Android Build Support module.
2. Clone this repository and open the project folder in Unity Hub.
3. Open File → Build Profiles, select Android and click Switch Platform.
4. Click Build.

## Technology

- Unity 6 with the Universal Render Pipeline (2D Renderer)
- C# with the legacy Input API for touch and the Input System for the back button
- TextMeshPro for text rendering
- IL2CPP scripting backend, ARM64

## Project structure

| Scene | Purpose |
| --- | --- |
| `DentineMenu` | Main menu, audio settings and intro video |
| `Screen_CharacterSelect` | Character selection |
| `Level1`, `Level2`, `Level3` | The three game levels |
| `Quiz` | Shared post-level quiz |
| `Diploma` | Completion screen |

## Evaluation

A preliminary pilot study with 25 parents and caregivers, acting as proxy evaluators, assessed the game's usability, accessibility and educational potential. Testing with children aged 3 to 5 is planned as future work.

## Author

Beatriz Leite, Universidade Fernando Pessoa

Supervisors: Prof. Nuno Ribeiro and Prof. Christophe Soares
