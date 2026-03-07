// // Tests/Editor/BattleCoreTests.cs
// // Run via: Unity Test Runner > Edit Mode
// // Pure C# — no UnityEngine dependency in logic under test.

// using NUnit.Framework;
// using System.Collections.Generic;
// using System.Linq;
// using DinosBattle.Core;
// using DinosBattle.Core.Enums;
// using DinosBattle.Core.Models;
// using DinosBattle.Systems.Combat;
// using DinosBattle.Systems.Turn;
// using DinosBattle.Systems.StatusEffects;
// using DinosBattle.Infrastructure.EventBus;

// namespace DinosBattle.Tests.Editor
// {
//     [TestFixture]
//     public class CombatUnitTests
//     {
//         private static CombatUnit MakeUnit(string name, int hp = 100, int atk = 20,
//                                             int spd = 10, TeamId team = TeamId.Player)
//         {
//             return new CombatUnit(name, team, 0, new StatBlock(hp, atk, 5, spd));
//         }

//         [Test]
//         public void ApplyDamage_ReducesHealth()
//         {
//             var unit = MakeUnit("Rex", hp: 100);
//             unit.ApplyDamage(30);
//             Assert.AreEqual(70, unit.CurrentHealth);
//         }

//         [Test]
//         public void ApplyDamage_ClampsAtZero()
//         {
//             var unit = MakeUnit("Rex", hp: 100);
//             unit.ApplyDamage(9999);
//             Assert.AreEqual(0, unit.CurrentHealth);
//             Assert.IsFalse(unit.IsAlive);
//         }

//         [Test]
//         public void RestoreHealth_ClampsAtMaxHealth()
//         {
//             var unit = MakeUnit("Rex", hp: 100);
//             unit.ApplyDamage(50);
//             unit.RestoreHealth(9999);
//             Assert.AreEqual(100, unit.CurrentHealth);
//         }

//         [Test]
//         public void OnDefeated_FiresWhenHealthReachesZero()
//         {
//             var  unit  = MakeUnit("Rex", hp: 50);
//             bool fired = false;
//             unit.OnDefeated += _ => fired = true;
//             unit.ApplyDamage(50);
//             Assert.IsTrue(fired);
//         }

//         [Test]
//         public void StatModifier_StacksNonDestructively()
//         {
//             var unit = MakeUnit("Rex", atk: 20);
//             Assert.AreEqual(20, unit.BaseStats.AttackPower);

//             unit.AddModifier(new StatModifier(attack: 10, duration: 2));
//             Assert.AreEqual(30, unit.EffectiveStats.AttackPower);
//             Assert.AreEqual(20, unit.BaseStats.AttackPower);  // base unchanged
//         }

//         [Test]
//         public void PoisonEffect_DealsDamageOnTurnEnd()
//         {
//             var unit   = MakeUnit("Rex", hp: 100);
//             var poison = new PoisonEffect(3, 0.10f);
//             unit.AddStatusEffect(poison);
//             unit.TickStatusEffects(false);
//             Assert.Less(unit.CurrentHealth, 100);
//         }

//         [Test]
//         public void StatusEffect_ExpiresAfterDuration()
//         {
//             var unit   = MakeUnit("Rex", hp: 200);
//             var poison = new PoisonEffect(2, 0.01f);
//             unit.AddStatusEffect(poison);
//             unit.TickStatusEffects(false);
//             unit.TickStatusEffects(false);
//             Assert.IsFalse(unit.HasStatus(StatusEffectType.Poison));
//         }

//         [Test]
//         public void Cooldown_TracksAndTicks()
//         {
//             var unit = MakeUnit("Rex");
//             unit.SetCooldown("Tail Whip", 3);
//             Assert.IsTrue(unit.IsOnCooldown("Tail Whip"));
//             unit.TickCooldowns();
//             unit.TickCooldowns();
//             unit.TickCooldowns();
//             Assert.IsFalse(unit.IsOnCooldown("Tail Whip"));
//         }
//     }

//     [TestFixture]
//     public class TurnSystemTests
//     {
//         private static CombatUnit MakeUnit(string name, int speed, TeamId team = TeamId.Player)
//         {
//             return new CombatUnit(name, team, 0, new StatBlock(100, 20, 5, speed));
//         }

//         [Test]
//         public void SpeedOrder_SortsDescending()
//         {
//             var units    = new[] { MakeUnit("Slow", 5), MakeUnit("Fast", 20), MakeUnit("Mid", 10) };
//             var strategy = new SpeedBasedTurnOrder();
//             var order    = strategy.BuildOrder(units);

//             Assert.AreEqual("Fast", order[0].Name);
//             Assert.AreEqual("Mid",  order[1].Name);
//             Assert.AreEqual("Slow", order[2].Name);
//         }

//         [Test]
//         public void TurnSystem_SkipsDeadUnits()
//         {
//             var bus    = new BattleEventBus();
//             var system = new TurnSystem(bus);
//             var units  = new[]
//             {
//                 MakeUnit("A", 20),
//                 MakeUnit("B", 15),
//                 MakeUnit("C", 10)
//             };

//             system.Initialize(units, new SpeedBasedTurnOrder());
//             Assert.AreEqual("A", system.Current.Name);

//             units[1].ApplyDamage(9999);  // Kill B

//             system.Advance();  // Should skip B → land on C
//             Assert.AreEqual("C", system.Current.Name);
//         }
//     }

//     [TestFixture]
//     public class DamageCalculatorTests
//     {
//         private static CombatUnit MakeUnit(int atk, int def, int hp = 100, TeamId team = TeamId.Player)
//         {
//             return new CombatUnit("TestUnit", team, 0, new StatBlock(hp, atk, def, 10));
//         }

//         [Test]
//         public void StandardCalculator_DamageIsPositive()
//         {
//             var calc     = new StandardDamageCalculator();
//             var attacker = MakeUnit(atk: 30, def: 5);
//             var defender = MakeUnit(atk: 10, def: 5, team: TeamId.Enemy);
//             var result   = calc.Calculate(attacker, defender, AbilityPayload.BasicAttack);

//             Assert.Greater(result.FinalDamage, 0);
//             Assert.IsFalse(result.IsMiss);
//         }

//         [Test]
//         public void FlatCalculator_AlwaysDealsExactAmount()
//         {
//             var calc     = new FlatDamageCalculator(25);
//             var attacker = MakeUnit(atk: 0, def: 0);
//             var defender = MakeUnit(atk: 0, def: 0, team: TeamId.Enemy);
//             var result   = calc.Calculate(attacker, defender, AbilityPayload.BasicAttack);

//             Assert.AreEqual(25, result.FinalDamage);
//         }

//         [Test]
//         public void DamageResult_MissFactory_HasZeroDamage()
//         {
//             var attacker = MakeUnit(10, 5);
//             var defender = MakeUnit(10, 5, team: TeamId.Enemy);
//             var result   = DamageResult.Miss(attacker, defender);

//             Assert.IsTrue(result.IsMiss);
//             Assert.AreEqual(0, result.FinalDamage);
//         }
//     }
// }
