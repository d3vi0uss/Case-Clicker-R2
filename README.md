# The Last Caretaker (Unity Mobile Horror Prototype)

A GitHub-ready 3D mobile horror game prototype for iPad, built around lightweight systems and modular scripts.

## Target Platform
- iPad (iOS)
- 60 FPS target on mid-range hardware
- Mobile-first constraints:
  - low-poly environment assets
  - baked lighting
  - minimal dynamic lights
  - no real-time shadows on gameplay lights
  - pooled procedural corridor pieces

## Folder Structure

```text
Assets/
    Scripts/
        Player/
        Systems/
        AI/
        Environment/
        UI/
    Prefabs/
    Materials/
    Scenes/
    Audio/
    Art/
```

## Included Scripts

### Player
- `MobilePlayerController.cs`
  - CharacterController-based first-person movement
  - Touch look support
  - Sprint and interaction support
  - Runtime speed multiplier hook (used by boiler penalties)
- `TouchInputManager.cs`
  - Left-side movement touch area
  - Right-side swipe look
  - UI button hooks:
    - `SetSprintHeld(bool)`
    - `TriggerInteract()`
    - `TriggerFlashlightToggle()`

### Systems
- `FlashlightController.cs`
  - Toggle flashlight
  - Battery drain
  - Low-battery intensity flicker
  - Mobile optimization: no shadows
- `SanitySystem.cs`
  - Hidden sanity value (0–100)
  - Drain from darkness and prolonged idle
  - Lightweight FOV wave distortion
  - Breathing audio intensity scaling
  - Occasional brief flashlight disturbance
- `BoilerSystem.cs`
  - Interactable repair console behavior
  - Countdown/neglect loop
  - Movement speed penalty when neglected
  - Repair coroutine to reset state

### AI
- `HotelAIManager.cs`
  - Tracks darkness, idle, and boiler neglect pressure
  - Weighted modular behavior trigger system
  - UnityEvent-driven to add new events without code rewrites

### Environment
- `CorridorGenerator.cs`
  - Prefab-based procedural corridor construction
  - Segment count cap
  - Object pooling
  - Optional turn variation
- `MirrorController.cs`
  - Low-res RenderTexture mirror
  - Controlled refresh rate
  - Optional delay/glitch effect

## Unity Scene Setup (Step-by-Step)

1. **Create a new URP or Built-in 3D project** (URP recommended for mobile control over quality).
2. **Create the folder structure** from above under `Assets/`.
3. **Create a `Main` scene** in `Assets/Scenes/`.
4. **Player setup**:
   - Add a `Player` GameObject with:
     - `CharacterController`
     - `MobilePlayerController`
   - Add child `Main Camera` and assign it in `MobilePlayerController`.
   - Add `TouchInputManager` to a separate `InputManager` object.
5. **UI setup (mobile controls)**:
   - Add a Canvas (Screen Space - Overlay).
   - Add buttons:
     - Sprint (hold): connect `PointerDown -> SetSprintHeld(true)`, `PointerUp -> SetSprintHeld(false)`
     - Interact: connect `OnClick -> TriggerInteract()`
     - Flashlight: connect `OnClick -> TriggerFlashlightToggle()`
   - Optional visual joystick image on left side (logic already uses left-screen touch).
6. **Flashlight setup**:
   - Add a child spotlight under camera.
   - Set Spot Light `Shadows = None`.
   - Attach `FlashlightController` and assign light + `TouchInputManager`.
7. **Sanity setup**:
   - Add `SanitySystem` to a systems object.
   - Assign camera, breathing `AudioSource`, player controller, and flashlight controller.
8. **Hotel AI setup**:
   - Add `HotelAIManager` to `GameSystems` object.
   - Populate behavior list with IDs such as:
     - `whisper_audio`
     - `flicker_lights`
     - `lock_door`
     - `intercom_voice`
   - Attach UnityEvents per behavior to your audio/light/door scripts.
9. **Boiler setup**:
   - Add a boiler console GameObject with collider.
   - Attach `BoilerSystem`.
   - Put console in the player's interaction layer mask.
10. **Corridor setup**:
    - Create several low-poly corridor segment prefabs in `Assets/Prefabs/`.
    - Add `CorridorGenerator` to a corridor root object.
    - Assign prefabs list and segment length.
11. **Mirror setup**:
    - Create a mirror plane and reflection camera.
    - Attach `MirrorController`, assign renderer and mirror camera.
    - Keep texture size low (e.g., 256).
12. **Lighting setup**:
    - Use baked GI.
    - Limit realtime lights.
    - Disable shadows on runtime lights.

## Example Prefab Setup

- **Corridor Segment Prefab**
  - Meshes combined where possible
  - 1–2 material slots max
  - Static marked for batching/light baking

- **Boiler Console Prefab**
  - Mesh + collider
  - `BoilerSystem`
  - Optional audio + animation hooks through UnityEvents

- **Door Prefab (for AI events)**
  - Collider + animator
  - A script exposing `LockTemporarily(float seconds)`
  - Referenced via `HotelAIManager` behavior UnityEvent

## iOS (iPad) Build Instructions

1. Install latest Unity iOS Build Support module.
2. Open `File > Build Settings`:
   - Platform: iOS
   - Add your main scene(s).
3. In `Project Settings > Player`:
   - Target SDK: Device SDK
   - Architecture: ARM64
   - Scripting Backend: IL2CPP
   - API Compatibility: .NET Standard 2.1
4. In `Project Settings > Quality`:
   - Use a dedicated Mobile quality tier.
   - Disable expensive effects (real-time shadows/post FX).
5. In `Project Settings > Graphics`:
   - Keep render pipeline lightweight.
6. Build to an Xcode project, open in Xcode, set signing team/profile, deploy to iPad.

## GitHub Setup Instructions

1. Initialize git (if needed):
   ```bash
   git init
   ```
2. Ensure `.gitignore` excludes Unity generated folders (`Library/`, `Temp/`, etc.).
3. Commit only source assets/scripts/scenes/settings.
4. Push to GitHub:
   ```bash
   git add .
   git commit -m "Initial prototype: The Last Caretaker mobile systems"
   git branch -M main
   git remote add origin <your-repo-url>
   git push -u origin main
   ```

## Performance Notes
- Prefer coroutines/timed loops over per-frame heavy logic.
- Keep draw calls low via static batching and mesh/material reuse.
- Keep mirror refresh capped and texture resolution low.
- Keep procedural generation bounded and pooled.
- Avoid runtime mesh generation on mobile.

