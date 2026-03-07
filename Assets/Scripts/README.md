# 🦕 DinosBattle V2 — Scalable Turn-Based Battle System
### Rebuilt with Production-Grade Architecture

---

## Design Patterns Used (& Why)

| Pattern | Where | Benefit |
|---|---|---|
| **Event Bus (Mediator)** | `BattleEventBus` | Zero coupling between all systems |
| **Service Locator / DI** | `BattleServiceLocator` / `BattleInstaller` | Swap any implementation with 1 line |
| **Strategy** | `ITurnOrderStrategy`, `IDamageCalculator`, `ITargetSelector` | Change game feel without touching orchestrator |
| **Command** | `IBattleCommand`, `BasicAttackCommand`, etc. | Undoable, replayable, loggable actions |
| **Observer** | `CombatUnit` events + EventBus | UI/Audio react without knowing about combat |
| **Factory** | `CombatUnitFactory` | Construction logic separated from entities |
| **Template Method** | `BaseStatusEffect`, `BaseAbility` | Shared structure, override only what differs |
| **Object Pool** | `ObjectPool<T>`, `GameObjectPool` | Zero GC from VFX/projectiles at scale |
| **MVP** | `BattleUIPresenter` | UI has zero combat knowledge |
| **Composition Root** | `BattleInstaller` | Only ONE place knows about concrete types |
| **Value Object** | `StatBlock`, `DamageResult`, `AbilityPayload` | Immutable, safe to pass and cache |
| **Decorator** | `StatModifier` stacking on `StatBlock` | Buffs/debuffs non-destructive |

---

## Architecture Diagram

```
                    ┌─────────────────────────┐
                    │     BattleInstaller      │  ← Composition Root
                    │  (Registers ALL services)│
                    └────────────┬────────────┘
                                 │ creates & registers
                    ┌────────────▼────────────┐
                    │   BattleServiceLocator   │
                    │  ITurnOrderStrategy      │
                    │  IDamageCalculator       │
                    │  ITargetSelector         │
                    │  IAnimationHandler       │
                    │  ITurnSystem             │
                    │  CombatResolver          │
                    │  CommandHistory          │
                    │  BattleEventBus ◄────────┼─── ALL events flow through here
                    └────────────┬────────────┘
                                 │ injected into
          ┌──────────────────────▼───────────────────────┐
          │              BattleCoordinator                │
          │  (thin orchestrator — drives state machine)   │
          │  Builds IBattleCommand → Execute() → advance  │
          └──────┬───────────┬───────────┬───────────────┘
                 │           │           │
          ┌──────▼──┐  ┌─────▼──┐  ┌────▼────────┐
          │Turn     │  │Combat  │  │Animation    │
          │System   │  │Resolve │  │Handler      │
          │(Strategy│  │r       │  │(IAnimation  │
          │pattern) │  │        │  │Handler)     │
          └─────────┘  └────────┘  └─────────────┘
                 │           │
          ┌──────▼──┐  ┌─────▼──────────────────┐
          │CombatUnit│  │DamageResult (value obj) │
          │(Observer)│  │AttackResult published   │
          │(Decorator│  │to EventBus              │
          │for stats)│  └─────────────────────────┘
          └──────────┘
                 │ events
          ┌──────▼─────────────────────────────────┐
          │         BattleEventBus subscribers      │
          │  BattleUIPresenter (MVP)                │
          │  BattleLogger                           │
          │  AudioManager (add: subscribe anywhere) │
          │  AnalyticsTracker (add: no code change) │
          └─────────────────────────────────────────┘
```

---

## Project Structure

```
DinosBattleV2/Scripts/
│
├── Core/
│   ├── Enums/BattleEnums.cs         ← All enums in one place
│   ├── Interfaces/IBattleInterfaces.cs ← ALL contracts defined here
│   ├── Models/BattleModels.cs       ← Immutable value types
│   └── CombatUnit.cs                ← Runtime entity (Observer + Decorator)
│
├── Infrastructure/
│   ├── EventBus/BattleEventBus.cs   ← Mediator pattern (pub/sub)
│   ├── ServiceLocator/              ← DI container (swap for Zenject)
│   ├── StateMachine/StateMachine.cs ← Generic FSM
│   ├── ObjectPool/ObjectPool.cs     ← Generic + GameObject pools
│   ├── BattleInstaller.cs           ← Composition Root ← SINGLE wiring point
│   └── BattleLogger.cs              ← IBattleLogger impl (swappable)
│
├── Data/
│   └── DinosaurData.cs              ← ScriptableObject (designer-facing)
│
├── Systems/
│   ├── BattleTeam.cs                ← Team aggregate root
│   ├── Turn/TurnSystem.cs           ← + 3 ITurnOrderStrategy implementations
│   ├── Combat/CombatResolver.cs     ← + 2 IDamageCalculator + 2 ITargetSelector
│   ├── StatusEffects/StatusEffects.cs ← Poison, Burn, Stun, Shield, Regen
│   ├── Abilities/Abilities.cs       ← TailWhip, HealRoar, PoisonBite, Rampage
│   ├── Animation/UnityAnimationHandler.cs ← IAnimationHandler impl
│   └── Spawn/CombatUnitFactory.cs   ← Factory pattern
│
├── Commands/
│   └── BattleCommands.cs            ← BasicAttack, UseAbility, TickStatus
│
├── Presenters/
│   └── BattleUIPresenter.cs         ← MVP Presenter (pure EventBus subscriber)
│
├── BattleCoordinator.cs             ← Thin orchestrator (replaces BattleManager)
│
└── Tests/Editor/BattleCoreTests.cs  ← NUnit tests (no UnityEngine dep)
```

---

## Scaling Examples

### Add a 6v6 battle
```csharp
// BattleCoordinator Inspector: drag 6 DinosaurData assets to each team array.
// Zero code changes required.
```

### Swap turn order to initiative rolls
```csharp
// BattleInstaller.cs — change ONE line:
[SerializeField] private TurnOrderMode turnOrderMode = TurnOrderMode.InitiativeRoll;
// SpeedBasedTurnOrder() → InitiativeRollTurnOrder()
```

### Add a new damage formula
```csharp
public class PercentHpDamageCalculator : IDamageCalculator
{
    public DamageResult Calculate(CombatUnit atk, CombatUnit def, AbilityPayload p)
    {
        int dmg = Mathf.RoundToInt(def.CurrentHealth * 0.25f);  // always 25% current HP
        return new DamageResult(atk, def, dmg, dmg, 0, false, false, p.OverrideDamageType);
    }
}
// Register in BattleInstaller — nothing else changes.
```

### Add a new status effect
```csharp
public class FreezeEffect : BaseStatusEffect
{
    public override string           EffectName => "Freeze";
    public override StatusEffectType Type       => StatusEffectType.Stun;   // reuse enum or extend it
    
    public FreezeEffect(int turns) : base(turns) { }
    
    public override void OnTurnStart(CombatUnit owner)
        => Debug.Log($"{owner.Name} is frozen solid!");
}
// Use: unit.AddStatusEffect(new FreezeEffect(2));
```

### Add a new ability
```csharp
public class ThunderClawAbility : BaseAbility
{
    public override string        AbilityName   => "Thunder Claw";
    public override int           CooldownTurns => 3;
    public override AbilityTarget Targeting     => AbilityTarget.SingleEnemy;

    public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
    {
        var target  = targets.FirstOrDefault(t => t.IsAlive);
        var payload = new AbilityPayload(this, DamageType.Electric, 1.8f);
        GetResolver().ResolveAttack(user, target, payload);
        target.AddStatusEffect(new StunEffect(1));
        yield break;
    }
}
```

### Add an audio system
```csharp
// AudioManager.cs — no changes to any other file:
public class AudioManager : MonoBehaviour
{
    private void OnEnable()
    {
        var bus = BattleServices.Get<BattleEventBus>();
        bus.Subscribe<AttackExecutedEvent>(e => PlayAttackSound(e.Result));
        bus.Subscribe<UnitDefeatedEvent>(e   => PlayDeathSound(e.Unit));
    }
}
```

### Add analytics
```csharp
public class BattleAnalytics : MonoBehaviour
{
    private void OnEnable()
    {
        var bus = BattleServices.Get<BattleEventBus>();
        bus.Subscribe<BattleEndedEvent>(e =>
            Analytics.CustomEvent("battle_completed", new Dictionary<string, object>
            {
                { "outcome", e.Outcome.ToString() },
                { "turns",   e.TotalTurns }
            }));
    }
}
```

---

## Running Tests
1. Open Unity Test Runner: **Window > General > Test Runner**
2. Select **Edit Mode**
3. Run `DinosBattle.Tests.Editor` — tests all core logic with zero Unity dependencies

---

## Upgrade to Zenject / VContainer
Replace `BattleInstaller.cs` composition with a proper DI Container:
```csharp
// VContainer example
public class BattleLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<BattleEventBus>(Lifetime.Scoped);
        builder.Register<SpeedBasedTurnOrder, ITurnOrderStrategy>(Lifetime.Scoped);
        builder.Register<StandardDamageCalculator, IDamageCalculator>(Lifetime.Scoped);
        builder.Register<CombatResolver>(Lifetime.Scoped);
        // etc.
    }
}
// All other files stay IDENTICAL — that's the power of interface-based DI.
```
