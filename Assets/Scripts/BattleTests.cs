// // Run via: Unity > Window > General > Test Runner > EditMode
// using NUnit.Framework;
// using System.Collections.Generic;
// using DinosBattle;
// using DinosBattle.Combat;

// namespace DinosBattle.Tests
// {
//     public class BattleTests
//     {
//         // ── Helpers ───────────────────────────────────────────────────────────

//         static CombatUnit MakeUnit(string name, int hp = 100, int atk = 20,
//                                     int def = 5, int spd = 10, TeamId team = TeamId.Player) =>
//             new CombatUnit(name, team, new StatBlock(hp, atk, def, spd));

//         // ── CombatUnit ────────────────────────────────────────────────────────

//         [Test]
//         public void Unit_StartsAtFullHealth()
//         {
//             var unit = MakeUnit("Rex");
//             Assert.AreEqual(100, unit.CurrentHealth);
//         }

//         [Test]
//         public void TakeDamage_ReducesHealth()
//         {
//             var unit = MakeUnit("Rex");
//             unit.TakeDamage(30);
//             Assert.AreEqual(70, unit.CurrentHealth);
//         }

//         [Test]
//         public void TakeDamage_CannotGoBelowZero()
//         {
//             var unit = MakeUnit("Rex", hp: 10);
//             unit.TakeDamage(999);
//             Assert.AreEqual(0, unit.CurrentHealth);
//             Assert.IsFalse(unit.IsAlive);
//         }

//         [Test]
//         public void Heal_CannotExceedMaxHealth()
//         {
//             var unit = MakeUnit("Rex", hp: 100);
//             unit.TakeDamage(20);
//             // unit.Heal(999);
//             Assert.AreEqual(100, unit.CurrentHealth);
//         }

//         // ── Status Effects ────────────────────────────────────────────────────

//         [Test]
//         public void PoisonEffect_DealsDamageEachTurnEnd()
//         {
//             var unit = MakeUnit("Rex", hp: 100);
//             unit.AddStatus(new PoisonEffect(2, 0.1f));   // 10 dmg/turn
//             unit.TickStatusEffects(false);               // turn end
//             Assert.AreEqual(90, unit.CurrentHealth);
//         }

//         [Test]
//         public void StunEffect_RegistersAsStunned()
//         {
//             var unit = MakeUnit("Rex");
//             unit.AddStatus(new StunEffect(1));
//             Assert.IsTrue(unit.HasStatus(StatusEffectType.Stun));
//         }

//         [Test]
//         public void StatusEffect_ExpiresAfterDuration()
//         {
//             var unit = MakeUnit("Rex");
//             unit.AddStatus(new StunEffect(1));
//             unit.TickStatusEffects(false);  // turn end ticks and expires
//             Assert.IsFalse(unit.HasStatus(StatusEffectType.Stun));
//         }

//         // ── Abilities ─────────────────────────────────────────────────────────

//         [Test]
//         public void Ability_GoesOnCooldownAfterUse()
//         {
//             var unit = MakeUnit("Rex");
//             unit.SetCooldown("Tail Whip", 3);
//             Assert.IsTrue(unit.IsOnCooldown("Tail Whip"));
//         }

//         [Test]
//         public void Cooldown_DecrementsEachTurn()
//         {
//             var unit = MakeUnit("Rex");
//             unit.SetCooldown("Tail Whip", 2);
//             unit.TickCooldowns();
//             unit.TickCooldowns();
//             Assert.IsFalse(unit.IsOnCooldown("Tail Whip"));
//         }

//         // ── DamageResult ──────────────────────────────────────────────────────

//         [Test]
//         public void DamageResult_MissHasZeroDamage()
//         {
//             var a = MakeUnit("A"); var b = MakeUnit("B", team: TeamId.Enemy);
//             var r = DamageResult.Miss(a, b);
//             Assert.IsTrue(r.IsMiss);
//             Assert.AreEqual(0, r.Damage);
//         }

//         // ── TurnSystem ────────────────────────────────────────────────────────

//         [Test]
//         public void TurnSystem_OrdersBySpeedDescending()
//         {
//             var slow = MakeUnit("Slow", spd: 5);
//             var fast = MakeUnit("Fast", spd: 15);
//             var ts   = new DinosBattle.Battle.TurnSystem();
//             ts.Initialize(new List<CombatUnit> { slow, fast });
//             Assert.AreEqual("Fast", ts.Current.Name);
//         }

//         [Test]
//         public void TurnSystem_SkipsDeadUnits()
//         {
//             var a = MakeUnit("A", spd: 20);
//             var b = MakeUnit("B", spd: 10);
//             var c = MakeUnit("C", spd: 5);
//             var ts = new DinosBattle.Battle.TurnSystem();
//             ts.Initialize(new List<CombatUnit> { a, b, c });

//             b.TakeDamage(999); // kill B
//             ts.Advance();      // should skip B, land on C
//             Assert.AreEqual("C", ts.Current.Name);
//         }
//     }
// }
