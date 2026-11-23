# YMH219-FlappyBird

A Flappy Bird style desktop game built for the **Nesne Tabanli Programlama (YMH 219)** course.

## Purpose
Practice timers, keyboard events, runtime movement of visual elements, collision
detection, and object-oriented design in a Windows Forms app.

## Technologies
- C# / net10.0-windows
- Windows Forms (GDI+ drawing, `Timer` game loop)
- Visual Studio 2022

## Project Structure
| File | Responsibility |
|------|----------------|
| `Bird.cs` | Player position, gravity, flap |
| `PipePair.cs` | Obstacle geometry and movement |
| `Game.cs` | State, score, speed/level, collisions, high score |
| `GameState.cs` | `Ready` / `Running` / `GameOver` |
| `MainForm.cs` | Window, input, timer loop, drawing |

## How to Run
1. Open `YMH219-FlappyBird.csproj` in Visual Studio 2022 (or run `dotnet run`).
2. Press **F5**.

## Controls
- **Space / Up arrow**: flap
- **R / Enter / "Yeni Oyun" button**: restart after Game Over

## Features
- Gravity and flap physics
- 3 recycled pipe obstacles
- Score, level-based speed increase
- High score saved to a file
- Game Over screen and restart

## Screenshots
<!-- Add at least 2 screenshots here -->
![Gameplay](screenshots/gameplay.png)
![Game Over](screenshots/gameover.png)
