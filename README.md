# Jurassic World (Unity) - Codebase Guide

This project is a turn-based dinosaur battle game built in Unity.

## Tech Stack

- Unity `2022.3.62f3`
- C# gameplay scripts under `Assets/Scripts`
- UI: uGUI + TextMeshPro
- Data-driven units via ScriptableObjects

## Game Flow

1. Scene starts at `Assets/Scenes/MainMenu.unity`
2. `GameStateManager` moves to `Assets/Scenes/BattleScene.unity` on Play
3. `BattleInstaller` registers core services
4. `BattleCoordinator` builds teams and runs the turn loop
5. `BattleHUD` renders HP/action/result UI from bus events
6. `GameStateManager` handles pause, victory/defeat, and scene transitions

## High-Level Architecture

- Orchestration: `BattleCoordinator` drives setup, turns, commands, and win checks
- DI-lite: `ServiceLocator` stores runtime services for one battle session
- Eventing: `EventBus` publishes battle events used by UI and systems
- Combat domain: `CombatUnit`, abilities, status effects, resolver, turn system
- Data: `DinosaurData` ScriptableObjects define unit stats + prefab
- Presentation: `BattleHUD`, `DinoAnimator`, menu/pause screen controllers

## Core Runtime Scripts (Active Path)

### Bootstrap and State

- `Assets/Scripts/GameStateManager.cs`
  - Global singleton (`DontDestroyOnLoad`)
  - Owns game state enum transitions (`MainMenu`, `Loading`, `Battle`, `Paused`, `Victory`, `Defeat`)
  - Handles `StartGame`, `PauseGame`, `ResumeGame`, `GoToMainMenu`, `QuitGame`

- `Assets/Scripts/BattleInstaller.cs`
  - Composition root for battle scene
  - Registers: `EventBus`, `ITargetSelector`, `IDamageCalculator`, `CombatResolver`, `TurnSystem`, `UnitFactory`, `PlayerInputHandler`
  - Clears services on destroy

- `Assets/Scripts/ServiceLocator.cs`
  - Static service container (`Register/Get/TryGet/Clear`)

### Battle Loop

- `Assets/Scripts/BattleCoordinator.cs`
  - Main battle coroutine:
    - Setup teams from `DinosaurData`
    - Publish `BattleStartedEvent`
    - Run turn loop
    - Tick statuses/cooldowns
    - Build and execute `IBattleCommand` (attack/ability)
    - Check win condition and publish `BattleEndedEvent`
  - Enemy AI: picks first usable ability, else basic attack
  - Player turn: waits for `PlayerInputHandler` action

- `Assets/Scripts/TurnSystem.cs`
  - Speed-based turn order
  - Rebuilds order each round from alive units
  - Skips dead units when advancing

### Combat Domain

- `Assets/Scripts/CombatUnit.cs`
  - Pure C# unit model (health, stats, cooldowns, statuses, abilities, animator/model refs)
  - Includes immutable `StatBlock`

- `Assets/Scripts/Interfaces.cs`
  - Contracts: `IAbility`, `IStatusEffect`, `IDamageCalculator`, `ITargetSelector`
  - Includes active `DamageResult` value struct

- `Assets/Scripts/CombatResolver.cs`
  - Damage calculators:
    - `StandardDamageCalculator`: `(ATK - DEF*0.5) * multiplier` + variance + crit
  - Target selectors:
    - `RandomTargetSelector`
    - `LowestHpTargetSelector`
  - Applies damage, defeat checks, and publishes combat/health events

- `Assets/Scripts/Abilities.cs`
  - `TailWhipAbility` (AOE damage)
  - `PoisonBiteAbility` (single-target + poison)
  - `HealRoarAbility` (self heal)
  - Base class: `BaseAbility`

- `Assets/Scripts/StatusEffects.cs`
  - `PoisonEffect`, `StunEffect`, `RegenEffect`
  - Base class: `BaseStatusEffect`
  - Applies turn-start/turn-end effects and publishes health/defeat events

- `Assets/Scripts/UnitFactory.cs`
  - Creates `CombatUnit` from `DinosaurData`
  - Auto-assigns starter abilities based on speed
  - Instantiates prefab model + links `DinoAnimator`
  - Supports custom spawn points

### Input and UI

- `Assets/Scripts/PlayerInputHandler.cs`
  - Bridge between UI buttons and battle loop commands
  - Emits `OnActionSubmitted`, `OnWindowOpened`, `OnWindowClosed`

- `Assets/Scripts/BattleHUD.cs`
  - Builds HP bars on `UnitRegisteredEvent`
  - Updates turn label, log, health bars, action buttons, result overlay
  - Subscribes to `EventBus` and `PlayerInputHandler`

- `Assets/Scripts/DinoAnimator.cs`
  - Plays animator triggers and simple motion effects (lunge, shake)
  - Handles attack/hurt/death/victory animation sequencing

### Events and Enums

- `Assets/Scripts/EventBus.cs`
  - Active battle event definitions and pub/sub implementation
  - Events include: battle start/end, turn start, attack, health, defeat, unit registered

- `Assets/Scripts/Enums.cs`
  - Shared enums: `TeamId`, `DamageType`, `StatusEffectType`, `AbilityTarget`, `AnimationType`, `BattleOutcome`, `GameState`

## Menus and Scene UI Controllers

- `Assets/Scripts/MainMenuScreen.cs` (`DinosBattle.UI.Screens`)
- `Assets/Scripts/PauseMenuScreen.cs` (`DinosBattle.UI.Screens`)
- `Assets/Scripts/MenuScreens.cs` (`DinosBattle.UI`)

Note: there are duplicate menu controller implementations (`MainMenuScreen`/`PauseMenuScreen`) in different namespaces. Keep only one namespace version in scene bindings to avoid confusion.

## Data and Content

- Unit data assets: `Assets/ScriptableObjects/Dinosaurs/*.asset`
  - `SO_TRex`, `SO_Velociraptor`, `SO_Triceratops`, `SO_Spinosaurus`
- Prefabs: `Assets/Prefabs`
  - dinosaur units, health bar prefab, ability button prefab
- Scenes:
  - `Assets/Scenes/MainMenu.unity`
  - `Assets/Scenes/BattleScene.unity`

## Utility and Legacy/Experimental Files

These files are present but not in the active battle wiring path above:

- `Assets/Scripts/CombatUnitFactory.cs` (fully commented old version)
- `Assets/Scripts/BattleEventBus.cs`
- `Assets/Scripts/BattleServiceLocator.cs`
- `Assets/Scripts/BattleModels.cs`
- `Assets/Scripts/IBattleInterfaces.cs`
- `Assets/Scripts/BattleEnums.cs` (commented)
- `Assets/Scripts/StateMachine.cs`
- `Assets/Scripts/ObjectPool.cs`
- `Assets/Scripts/BattleTests.cs` (commented tests)
- `Assets/Scripts/Editor/uGUITools.cs` (editor utility menu commands)

These appear to be part of an alternate/earlier architecture and can be used as reference, but current runtime code uses `ServiceLocator + EventBus + BattleCoordinator` in `DinosBattle` namespaces.

## Folder Map (Condensed)

- `Assets/Scripts` - gameplay/runtime/editor C# code
- `Assets/Scenes` - playable scenes
- `Assets/ScriptableObjects` - designer data assets
- `Assets/Prefabs` - UI + dinosaur prefabs used in battle
- `Assets/Font`, `Assets/TextMesh Pro` - fonts and TMP setup
- `Assets/* Pack folders` - third-party art/model/environment assets

## How to Run

1. Open project in Unity `2022.3.62f3`
2. Open `Assets/Scenes/MainMenu.unity`
3. Press Play
4. From menu, start battle and validate turn/ability/pause/result flows

## Extension Points

- Add dinosaur: create new `DinosaurData` asset and assign prefab/stats
- Add ability: implement `IAbility` (or subclass `BaseAbility`)
- Add status effect: implement `IStatusEffect` (or subclass `BaseStatusEffect`)
- Change AI targeting: switch `BattleInstaller.TargetMode` or add a new `ITargetSelector`
- Change damage formula: provide a new `IDamageCalculator` and register it in `BattleInstaller`
