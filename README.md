# Endless Crowd Runner 3D

A complete, production-ready, endless crowd-runner game built in **Unity (C#, URP, 3D Low-Poly, Portrait Orientation)**. Inspired by popular "crowd master / mob control" mobile games, where every advertised mechanic is fully implemented and genuinely playable: dynamic crowd formations, math gates (+, -, *, /), melee mob clashes, enemy gunners, weapon upgrades (Pistol -> SMG -> Shotgun -> AR -> Minigun), squad pickups, lethal obstacles, boss encounters with floating health bars, procedural audio, and continuous difficulty scaling.

---

## Table of Contents
1. [Project Overview & Core Mechanics](#project-overview--core-mechanics)
2. [Unity Version & Requirements](#unity-version--requirements)
3. [Quick Setup & Play Instructions](#quick-setup--play-instructions)
4. [Android Build Guide (APK & Google Play AAB)](#android-build-guide-apk--google-play-aab)
5. [Tuning Difficulty & Game Balancing](#tuning-difficulty--game-balancing)
6. [Architecture & Script Directory](#architecture--script-directory)
7. [Performance Optimizations (60 FPS on Mobile)](#performance-optimizations-60-fps-on-mobile)
8. [Known Limitations & Suggested Next Steps](#known-limitations--suggested-next-steps)

---

## Project Overview & Core Mechanics

### 1. Crowd Control & Formation
- **Lane-less Road**: Smooth touch or mouse horizontal drag clamped to road boundaries.
- **Dynamic Organic Formation**: Uses Fermat's spiral (phyllotaxis disk packing) to pack units tightly without overlap. As count changes, the crowd smoothly expands or contracts.
- **Procedural Stickman Animation**: 100% code-driven running bob, tilt, squish, and wobble. Zero Animator Controller overhead, allowing 300+ units on screen simultaneously at 60 FPS.
- **Above-Crowd Counter**: Floating real-time count billboard following the crowd centroid.

### 2. Interactive Math Gates
- **Gate Types**:
  - **Addition (`+X`)**: e.g., `+5, +10, +25, +50, +100` (Bright Green)
  - **Multiplication (`xX`)**: e.g., `x2, x3, x5` (Bright Green)
  - **Subtraction (`-X`)**: e.g., `-5, -10, -25, -50` (Red/Orange)
  - **Division (`/X`)**: e.g., `/2, `/3, `/4, `/5` (Red/Orange, integer division with floor, clamped at 0)
- **Overlap Detection**: Calculates horizontal crowd overlap score so the crowd triggers the gate it passes through most.
- **Juice**: Scale bounce ripple on impact, floating `+X` or `-X` text popups, unit spawn bursts, and death particles.

### 3. Combat & Enemy Mobs
- **Enemy Groups**: Red mob standing on the road displaying a 3D count badge.
- **Melee Clash**: Player and enemy units cancel each other 1:1 on contact until one side is eliminated.
- **Enemy Gunners**: Backline shooters detect player crowd from 36m away and fire red tracer bullets, killing player units before contact.
- **Boss Battles**: Occur every ~60 seconds. A giant titan blocks the road with a massive floating HP bar, executing ground slams. The crowd can shoot it down from distance or clash through it. Defeating the boss awards `+30 Squad` and `+1500 Score`.

### 4. Weapon Upgrades & Pickups
- **Weapon Progression**: `Unarmed -> Pistol -> SMG -> Shotgun -> Assault Rifle -> Minigun`.
- **Auto-Targeting**: Armed stickmen auto-target enemies and bosses in range with visible tracer projectiles and hit sparks.
- **Squad Pickups**:
  - **+10 Squad**: Adds 10 stickmen immediately.
  - **Heavy Gunner**: Adds a large armored unit worth 5 HP.

### 5. Obstacles
- **Spinning Blades**: Rotating blades sweeping across the roadway that slice units.
- **Walls with Gaps**: Solid walls with a narrow passage; units outside the gap are crushed.
- **Pits**: Holes in the road where units falling in are lost.

### 6. Procedural Audio
- 100% royalty-free procedural audio synthesized at runtime using mathematical waveforms (`AudioClip.Create`). No external sound asset dependencies required!

---

## Unity Version & Requirements

- **Recommended Unity Versions**:
  - **Unity 2022.3 LTS** (e.g., 2022.3.20f1 or newer)
  - **Unity 2021.3 LTS** (e.g., 2021.3.30f1 or newer)
  - **Unity 6 (6000.0.x)**
- **Render Pipeline**: Universal Render Pipeline (URP).
- **Target Platform**: Android (API Level 24+ / Android 7.0+, Target API 34).

---

## Quick Setup & Play Instructions

1. **Open Project in Unity**:
   - Open **Unity Hub**.
   - Click **Add** -> **Add project from disk**.
   - Select the `EndlessCrowdRunner` folder.
   - Select your installed Unity Editor version (2022.3 LTS or 6000.x recommended) and open.
2. **Open the Scene**:
   - In the Unity Project window, navigate to `Assets/Scenes/MainScene.unity` and double-click to open.
3. **Run the Game**:
   - In the Unity Editor, set the Game view aspect ratio to **Portrait** (e.g., `9:16` or `1080 x 1920`).
   - Click the **Play** button!
   - Click **TAP TO RUN** on the main menu.
   - Drag mouse left/right to steer the crowd.

> **Tip**: If you ever create a blank scene or want to verify the setup, use the Editor menu:  
> `Tools -> Endless Crowd Runner -> Setup Game Scene`.

---

## Android Build Guide (APK & Google Play AAB)

### Prerequisites
1. Ensure the **Android Build Support** module is installed in Unity Hub (including **Android SDK & NDK Tools** and **OpenJDK**).

### Build Steps
1. In Unity, go to **File -> Build Settings...**.
2. Select **Android** from the Platform list and click **Switch Platform**.
3. Verify `Assets/Scenes/MainScene.unity` is checked in the **Scenes In Build** list.
4. Click **Player Settings...** (or go to `Edit -> Project Settings -> Player`):
   - **Orientation**: Already configured to `Portrait`.
   - **Package Name**: `com.playgames.endlesscrowdrunner` (or your company identifier).
   - **Minimum API Level**: Android 7.0 'Nougat' (API Level 24).
   - **Target API Level**: Automatic (highest installed, API 33/34 for Google Play).
   - **Scripting Backend**: Set to **IL2CPP**.
   - **Target Architectures**: Check **ARMv7** and **ARM64**.
5. **For Testing on Device (APK)**:
   - Uncheck `Build App Bundle (Google Play)`.
   - Click **Build** (or **Build and Run** with your Android device connected via USB with USB Debugging enabled).
   - Select output filename (e.g. `EndlessCrowdRunner.apk`).
6. **For Google Play Store Release (AAB)**:
   - Check **Build App Bundle (Google Play)**.
   - In **Publishing Settings**, create or assign your `.keystore` signing key.
   - Click **Build** to produce the `.aab` file ready for the Google Play Console.

---

## Tuning Difficulty & Game Balancing

All gameplay balance parameters are centralized in `GameConfig` and can be adjusted without touching any C# code!

### Accessing the Config:
- In the Project window, select `Assets/Resources/GameConfig.asset` (or inspect the `GameConfig` asset).

| Setting | Default | Description |
| :--- | :--- | :--- |
| `baseRunSpeed` | `11.0` | Initial forward running speed (m/s). |
| `maxRunSpeed` | `22.0` | Maximum speed cap reached at high distances. |
| `speedAccelerationPer100m` | `0.5` | Running speed increase every 100 meters. |
| `dragSensitivity` | `1.35` | Touch / mouse steering sensitivity. |
| `roadWidth` | `10.0` | Width of the playable road. |
| `startingCount` | `1` | Starting number of stickmen. |
| `maxCrowdCapacity` | `600` | Hard cap on total units rendered. |
| `unitSpacing` | `0.65` | Distance between stickmen in formation. |
| `baseEnemyGroupCount` | `8` | Starting enemy mob size. |
| `enemyGrowthPer100m` | `3.0` | Extra enemies added per 100 meters. |
| `bossIntervalSeconds` | `60.0` | Time interval between boss spawns. |
| `bossBaseHp` | `250` | HP of the first boss encounter. |
| `bossHpGrowthPerTier` | `160` | Additional HP per subsequent boss. |
| `generationSeed` | `0` | `0` for random runs; set a fixed integer (e.g., `12345`) to replay the exact same procedural road layout for debugging or tournaments. |

---

## Architecture & Script Directory

```
Assets/Scripts/
├── Audio/
│   ├── SoundEffectType.cs            # Sound effect identifiers
│   ├── ProceduralAudioGenerator.cs   # Mathematical waveform synthesizer (zero external audio dependencies)
│   └── SoundManager.cs               # Audio pooling, volume, and mute persistence
├── Camera/
│   ├── CameraFollow.cs               # Dynamic portrait framing & count-based zoom
│   └── CameraShake.cs                # Procedural impulse screen shake
├── Core/
│   ├── GameState.cs                  # MainMenu, Playing, Paused, GameOver states
│   ├── GameManager.cs                # Central state & high-score coordinator
│   └── RuntimeBootstrapper.cs        # Auto-wires missing scene components on startup
├── Crowd/
│   ├── FormationHelper.cs            # Fermat's spiral disk packing algorithm
│   ├── StickmanAnimator.cs           # High-performance procedural running animation
│   ├── StickmanUnit.cs               # Individual stickman logic, pooling, and health
│   └── CrowdController.cs            # Movement, touch drag, math gates, and unit lists
├── Difficulty/
│   ├── GameConfig.cs                 # ScriptableObject configuration for zero-code tuning
│   └── DifficultyScaler.cs           # Distance tracking & continuous difficulty scaling
├── Enemies/
│   ├── EnemyUnit.cs                  # Red enemy stickman
│   ├── EnemyShooter.cs               # AI shooter firing tracer bullets at player crowd
│   ├── EnemyGroup.cs                 # Enemy mob, count badge, and melee collision
│   └── BossController.cs             # Giant boss, health bar, ground slam, and rewards
├── Environment/
│   ├── ChunkType.cs                  # Runway, GateRow, Enemy, Mixed, Obstacle, Reward, Boss
│   ├── RoadChunk.cs                  # Segment recycling & child item management
│   ├── ChunkGenerator.cs             # Weighted endless generation with fairness rules
│   └── Obstacles/
│       ├── SpinningBlade.cs          # Rotating lethal blade
│       ├── WallObstacle.cs           # Wall with passable gap opening
│       └── PitObstacle.cs            # Pit trap hazard
├── Gates/
│   ├── GateType.cs                   # Add, Multiply, Subtract, Divide
│   ├── Gate.cs                       # Gate visuals, math display, and overlap scoring
│   └── GateGroup.cs                  # Multi-gate row coordination & fairness enforcement
├── Pickups/
│   ├── PickupType.cs                 # WeaponUpgrade, Squad10, HeavyGunner
│   ├── WeaponPickup.cs               # Weapon upgrade crate
│   └── SquadPickup.cs                # Bonus stickmen beacon
├── Pooling/
│   ├── IPoolable.cs                  # Fast activation/reset interface
│   ├── ObjectPool.cs                 # Non-allocating generic object pool
│   └── PoolManager.cs                # Central pre-warmed pool coordinator
├── UI/
│   ├── HUDView.cs                    # Crowd count, distance, best score, weapon badge
│   ├── MainMenuView.cs               # Title, play button, sound toggle
│   ├── PauseMenuView.cs              # Resume, restart, audio toggle
│   ├── GameOverView.cs               # Final score, best score, new record badge, retry
│   ├── FloatingText.cs               # Floating text popups (+10, -5, CLEARED)
│   ├── FloatingTextManager.cs        # Floating text pool manager
│   ├── DebugFPSOverlay.cs            # Real-time mobile diagnostic overlay
│   └── UIManager.cs                  # UI transition manager & procedural canvas builder
├── Visuals/
│   ├── ParticleBurst.cs              # Pooled particle bursts
│   └── VisualAssetFactory.cs         # Low-poly mesh builders & GPU-instanced materials
├── Weapons/
│   ├── WeaponType.cs                 # None, Pistol, SMG, Shotgun, AR, Minigun
│   ├── WeaponData.cs                 # Damage, fire rate, bullet speed, pellet count
│   ├── WeaponSystem.cs               # Auto-targeting & volley firing
│   └── Bullet.cs                     # Pooled projectile with non-alloc hit checks
└── Editor/
    └── SceneSetupWizard.cs           # One-click scene builder & validator in Unity Editor
```

---

## Performance Optimizations (60 FPS on Mobile)

1. **Object Pooling**: Every stickman unit, enemy, bullet, particle burst, floating text, and road chunk is pooled via `ObjectPool<T>` and `PoolManager`. No runtime `Instantiate()` or `Destroy()` calls during gameplay.
2. **GPU Instancing**: All procedural materials created in `VisualAssetFactory` have `enableInstancing = true` enabled, batching hundreds of stickmen into single draw calls.
3. **Procedural Animation**: Replaces heavy Animator/Mecanim state machines with lightweight mathematical calculations in `StickmanAnimator.cs`, saving substantial CPU cycles.
4. **Zero GC per Frame**: Core loops use pre-allocated buffers and non-allocating physics (`Physics.OverlapSphereNonAlloc`) to eliminate garbage collection hitching.
5. **Real-time Diagnostics**: Includes `DebugFPSOverlay.cs` displaying live FPS, total on-screen unit count, active bullets, and distance. (Can be toggled by tapping the top-left screen corner).

---

## Known Limitations & Suggested Next Steps

### Known Limitations
1. **Low-Poly Primitive Visuals**: Units use stylized procedural capsule/sphere stickmen and solid-color instanced shaders. While clean and highly performant, custom 3D rigged humanoid models and skinned mesh instancing can be swapped in if desired.
2. **Flat Endless Plane**: Road segments are currently straight along the Z axis. Curved spline roads or multi-level ramps can be integrated by extending `RoadChunk.cs`.

### Suggested Next Steps & Monetization
1. **Ad Integration**:
   - **Rewarded Video**: "Revive once with +50 Stickmen" when the crowd is wiped out.
   - **Interstitial Ads**: Displayed every 3-4 runs between Game Over and Retry.
   - Recommended plugins: Unity Ads or Google Mobile Ads (AdMob).
2. **Cosmetic Skins & Hats**:
   - Unlockable stickman skins (Ninja, Knight, Astronaut, Robot) using collected coins or distance milestones.
3. **Daily Rewards & Lucky Spin Wheel**:
   - Daily login bonus rewarding starting weapon upgrades or coins.
4. **Haptic Feedback**:
   - Android vibration (`Handheld.Vibrate()`) on gate impacts, weapon fire, and boss explosions.
