# Dream Blast

A level-based mobile puzzle game developed in Unity and C# where players must clear obstacles by matching and blasting colored cubes and using special rockets.

## Game Overview

Dream Blast is a matching puzzle game with the following key features:

- Level-based progression system
- Four types of colored cubes that can be matched and blasted
- Special rockets that can be created and combined
- Various obstacles to clear (boxes, stones, and vases)
- Persistent level progress
- Celebration animations for level completion
- Fail screen with retry options

## Gameplay Mechanics

### Core Mechanics
- **Grid System**: Rectangular grid (6-10 cells width/height) where each cell can contain one item
- **Matching**: Blast cubes by tapping groups of 2+ adjacent same-colored cubes
- **Gravity**: Cubes and certain objects fall to fill empty spaces
- **Move Limit**: Each level has a limited number of moves to complete objectives

### Special Items
- **Rockets**: Created when 4+ cubes are blasted in a single move
  - Can be horizontal or vertical
  - Explode when tapped or hit by another rocket
  - Divide into 2 moving parts that damage cells along their path
- **Rocket Combos**: Adjacent rockets can be combined for special effects
  - Rocket-Rocket combo creates 3x3 explosions in both directions

### Obstacles
- **Box**: Cleared by one adjacent blast or rocket hit
- **Stone**: Only damaged by rockets, cleared with one hit
- **Vase**: Requires two separate damages to clear, can fall to fill empty spaces

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/              # Core game systems
│   ├── Data/              # Data loader classes
│   ├── Interfaces/        # Common interfaces
│   ├── UI/                # UI GameObject Scripts
├── Scenes/
│   ├── MainScene.unity    # Main menu scene
│   └── LevelScene.unity   # Gameplay scene
├── Prefabs/               # Reusable game objects
├── Resources/
│   └── Levels/            # Level definition files
└── Animations/            # Animation assets
```

## Technical Implementation

- **Unity Version**: 6000.0.32f1 with built-in renderer
- **Device Support**: Portrait orientation (9:16)
- **State Management**: Local persistence of player progress

## Setup Instructions

1. Clone the repository
2. Open the project in Unity 6000.0.32f1 or later
3. Open the MainScene to start the game
4. Play the game in the Unity Editor

## Level System

The game includes 10 predefined levels with increasing difficulty. Each level is defined with:
- Grid dimensions
- Move count
- Initial grid layout with different items

## Custom Editor Tools

The project includes a custom Unity editor menu for:
- Setting the player's current level
- Testing specific level configurations

## Dependencies

- LeanTween

## Credits

Developed as part of the Dream Games Software Engineering Study.
