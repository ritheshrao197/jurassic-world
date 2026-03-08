# DinosBattle Codebase Documentation

This document describes the current codebase under `Assets/Scripts` and how runtime systems work together.

## Project Snapshot

- Engine: Unity `2022.3.62f3`
- Main scenes:
  - `Assets/Scenes/MainMenu.unity`
  - `Assets/Scenes/BattleScene.unity`
- Build settings order:
  1. `MainMenu`
  2. `BattleScene`
- Core gameplay model: turn-based dinosaur combat with teams, abilities, status effects, UI-driven player actions, and simple AI.

## Scripts Folder Structure

`Assets/Scripts` is organized by feature:

- `Animation`
  - `DinoAnimator.cs`
- `Battle`
  - `BattleCoordinator.cs`
  - `BattleInstaller.cs`
  - `ServiceLocator.cs`
  - `TurnSystem.cs`
- `Combat`
  - `Abilities.cs`
  - `CombatResolver.cs`
  - `StatusEffects.cs`
- `Core`
  - `CombatUnit.cs`
  - `Enums.cs`
  - `EventBus.cs`
  - `Interfaces.cs`
  - `ObjectPool.cs`
- `Data`
  - `DinosaurData.cs`
  - `UnitFactory.cs`
- `Editor`
  - `uGUITools.cs`
- `Game`
  - `GameStateManager.cs`
- `Input`
  - `PlayerInputHandler.cs`
- `UI`
  - `BattleHUD.cs`
  - `MainMenuScreen.cs`
  - `PauseMenuScreen.cs`

## Runtime Architecture

### 1. Composition root and dependency wiring

`BattleInstaller` runs early (`DefaultExecutionOrder(-200)`) in battle scene and registers services into `ServiceLocator`:

- `EventBus`
- `ITargetSelector` (`RandomTargetSelector` or `LowestHpTargetSelector`)
- `IDamageCalculator` (`StandardDamageCalculator`)
- `CombatResolver`
- `TurnSystem`
- `UnitFactory`
- `PlayerInputHandler` (adds component if missing)

This creates one clear place where runtime implementations are selected.

### 2. Battle orchestration

`BattleCoordinator` (`DefaultExecutionOrder(-100)`) is the central loop controller:

1. Resolves dependencies from `ServiceLocator`
2. Builds teams from `DinosaurData[]` via `UnitFactory`
3. Publishes `UnitRegisteredEvent` for each unit
4. Initializes `TurnSystem` with player/enemy lists
5. Publishes `BattleStartedEvent`
6. Runs turn loop until battle over
7. Publishes `BattleEndedEvent` and notifies `GameStateManager`

The loop is coroutine-based, not `Update` polling, and each turn:

- publishes `TurnStartedEvent`
- ticks start-of-turn status effects
- skips stunned units
- gets player command from UI input or AI command for enemy
- executes command (attack/ability)
- ticks end-of-turn statuses and cooldowns
- advances turn
- checks win condition

### 3. Data model and combat domain

`CombatUnit` is a pure C# entity:

- identity: `Name`, `Team`
- stats: immutable `StatBlock`
- runtime state: `CurrentHealth`, alive flag, abilities, statuses, cooldowns
- visual links: `Model` and `DinoAnimator`

Key responsibilities:

- health operations (`TakeDamage`, `Heal`)
- status lifecycle (`AddStatus`, `TickStatusEffects`, expiration handling)
- cooldown lifecycle (`SetCooldown`, `TickCooldowns`)

### 4. Turn management

`TurnSystem` builds an alternating queue:

- alive players sorted by speed descending
- alive enemies sorted by speed descending
- queue interleaves: player, enemy, player, enemy...
- if team sizes differ, extra units append at end of round
- dead units are skipped while advancing
- queue rebuilds each round

This supports alternating team turns while preserving speed priority inside each team.

### 5. Combat resolution and targeting

`CombatResolver` owns damage application and event publishing:

- selects target via `ITargetSelector`
- computes damage via `IDamageCalculator`
- applies damage
- emits `AttackExecutedEvent`, `HealthChangedEvent`, and `UnitDefeatedEvent` when needed
- logs combat details with `Debug.Log`

Current strategies:

- `StandardDamageCalculator`: `(Attack - Defense * 0.5) * multiplier`, random variance, crit chance/multiplier
- `RandomTargetSelector`: random alive target
- `LowestHpTargetSelector`: deterministic low-HP focus

### 6. Abilities and status effects

Abilities (`Abilities.cs`):

- `TailWhipAbility`: AoE hit to all enemies, reduced power
- `PoisonBiteAbility`: single-target boosted hit + poison
- `HealRoarAbility`: self-heal by max HP percentage
- `BaseAbility`: shared cooldown/availability shape

Status effects (`StatusEffects.cs`):

- `PoisonEffect`: damage over time at turn end
- `StunEffect`: action denial for turns
- `RegenEffect`: heal at turn start
- `BaseStatusEffect`: lifecycle hooks (`OnApply`, `OnTurnStart`, `OnTurnEnd`, `OnRemove`) and stacking

### 7. Input and command execution

`PlayerInputHandler` is a bridge between UI and battle loop:

- opens/closes input windows
- tracks active acting unit
- accepts attack or ability submission
- validates ability index/cooldown before dispatch

`BattleCoordinator` converts selected action to command objects:

- `AttackCommand`
- `AbilityCommand`

Both commands are coroutine-based and include animation playback steps.

### 8. Animation and presentation

`DinoAnimator` drives action feedback:

- triggers animator states (`Attack`, `Hurt`, `Death`, `Victory`)
- optional sound effects
- procedural lunge for attack/ability and shake for hurt

`BattleHUD` is event-driven UI:

- subscribes to `EventBus`
- creates HP bars on unit registration
- updates turn label, health bars, battle log
- shows player action buttons when input window opens
- enables/disables ability buttons by cooldown
- shows result overlay on battle end

Menu screens:

- `MainMenuScreen`: play/quit wiring + fade-in
- `PauseMenuScreen`: pause panel, state-reactive show/hide, Escape toggle

### 9. Global state and scene transitions

`GameStateManager` is a scene-persistent singleton:

- states: `MainMenu`, `Loading`, `Battle`, `Paused`, `Victory`, `Defeat`
- transitions scenes for start game/main menu
- controls `Time.timeScale` for pause/resume
- broadcasts `OnStateChanged`
- handles editor/runtime quit paths

## Event Model

Defined in `Core/EventBus.cs`:

- `BattleStartedEvent`
- `BattleEndedEvent`
- `UnitRegisteredEvent`
- `TurnStartedEvent`
- `AttackExecutedEvent`
- `UnitDefeatedEvent`
- `HealthChangedEvent`

Events decouple battle logic from UI and any future subsystems (audio, analytics, VFX trackers).

## Data Assets and Scene Wiring

### Dinosaur data assets

ScriptableObjects under `Assets/ScriptableObjects/Dinosaurs`:

- `SO_TRex.asset`
- `SO_Velociraptor.asset`
- `SO_Triceratops.asset`
- `SO_Spinosaurus.asset`

Each `DinosaurData` contains:

- identity (`dinoName`, portrait, prefab)
- stats (`maxHealth`, `attack`, `defense`, `speed`, crit fields)

### Prefabs used by scripts

From `Assets/Prefabs`:

- `HealthBarPrefab.prefab`
- `AbilityButtonPrefab.prefab`
- dinosaur prefabs (e.g., `Trex.prefab`, `triceraptor.prefab`, `velocirAPTR.prefab`, etc.)

`UnitFactory` instantiates dinosaur prefabs and binds `DinoAnimator`.

## Extending the System

### Add a dinosaur

1. Create new `DinosaurData` asset
2. Assign model prefab and stats
3. Add asset to `playerTeam` or `enemyTeam` in `BattleCoordinator`
4. Add spawn point transforms as needed

### Add a new ability

1. Implement `IAbility` (or derive from `BaseAbility`)
2. Add logic in `Execute`
3. Attach ability to units (currently done in `UnitFactory`)

### Add a new status effect

1. Implement `IStatusEffect` (or derive from `BaseStatusEffect`)
2. Apply via ability or combat logic
3. Publish health/defeat events as needed for UI sync

### Change targeting or damage rules

- Targeting: create a new `ITargetSelector`, register it in `BattleInstaller`
- Damage: create a new `IDamageCalculator`, register it in `BattleInstaller`

## Notes on Legacy/Utility Files

- `Core/BattleEventBus.cs` is commented-out legacy code (inactive).
- `Core/ObjectPool.cs` provides generic and GameObject pools; currently not wired into battle flow.
- `Editor/uGUITools.cs` is editor-only utility for RectTransform anchor tools.

## Current Design Characteristics

- Modular and non-monolithic
- Minimal update-loop coupling in battle logic
- Scales to larger team sizes through list/array-based processing
- Event-driven UI updates
- Clear composition root for strategy swapping

## Suggested Maintenance Practices

- Keep `BattleInstaller` as single wiring point
- Add EditMode tests for `TurnSystem`, `CombatResolver`, and status stacking logic
- Prefer adding features via interfaces (`IAbility`, `IStatusEffect`, `ITargetSelector`, `IDamageCalculator`) to keep coordinator thin
