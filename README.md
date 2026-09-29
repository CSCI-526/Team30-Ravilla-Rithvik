# Beat Boop

A 2D rhythm platformer made in Unity for CSCI 526 (Team 30). Moving and jumping work normally, but your special move, the **dash**, only works if you press it on the beat.

## Controls
| Key | Action |
|---|---|
| A / D or ← / → | Move |
| W or ↑ | Jump |
| Space | Dash (on the beat) |

## How the dash works
A ring around the player closes in on each beat and turns gold when a press would count as Perfect. By default the dash takes **two presses on consecutive beats**: the first press charges it and the second fires it in the direction you're facing. The dash only goes the full distance, and only breaks blue enemies, if **both** presses are Perfect. Missing the beat drops the charge.

Other dash modes (single beat, double tap) can be switched on the Player's `PlayerDash` component for testing.

## Levels
Start from `MainMenu`, then play `Level1` → `Level2` → `Level3`. The levels include checkpoints, hazards (spikes, saws, crushers, lava), platforms that appear or move on the beat, and tempo zones that change the music's speed.

## Running the project
1. Open the folder in **Unity 6000.3.22f1** through Unity Hub.
2. Open `Assets/Scenes/MainMenu.unity` and press Play.

## Project layout
```
Assets/
├── Scenes/     MainMenu, Level1–3, BeatTest (beat-timing test scene)
├── Prefabs/    Player, BeatSystem, HUD, blocks and platforms, hazards, enemies, checkpoints
└── Scripts/
    ├── Beat/       Beat clock, timing judge, metronome, pulse ring (see Beat/README.md)
    ├── Player/     Movement, dash, health, respawn
    ├── Level/      Checkpoints, hazards, enemies, beat platforms, tempo zones
    ├── Platforms/  Moving platforms
    ├── Camera/     Camera follow
    └── UI/         Menu, HUD, judgement popups
```
