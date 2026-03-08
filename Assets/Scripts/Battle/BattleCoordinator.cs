using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DinosBattle.Battle;
using DinosBattle.Combat;
using DinosBattle.Data;
using DinosBattle.Input;

namespace DinosBattle
{
    // Drives the battle loop: setup → turns → win check.
    // Pulls all dependencies from ServiceLocator (wired by BattleInstaller).
    [DefaultExecutionOrder(-100)]
    public class BattleCoordinator : MonoBehaviour
    {
        [Header("Teams")]
        [SerializeField] private DinosaurData[] playerTeam;
        [SerializeField] private DinosaurData[] enemyTeam;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] playerSpawns;
        [SerializeField] private Transform[] enemySpawns;

        [Header("Pacing")]
        [SerializeField] private float enemyDelay  = 1.2f;
        [SerializeField] private float actionDelay = 0.5f;

        private EventBus           _bus;
        private TurnSystem         _turns;
        private CombatResolver     _resolver;
        private ITargetSelector    _selector;
        private UnitFactory        _factory;
        private PlayerInputHandler _input;

        private List<CombatUnit> _players = new List<CombatUnit>();
        private List<CombatUnit> _enemies = new List<CombatUnit>();

        private int  _turn;
        private bool _over;

        private IBattleCommand _pendingCmd;
        private bool           _cmdReady;

        private void Start()
        {
            _bus      = ServiceLocator.Get<EventBus>();
            _turns    = ServiceLocator.Get<TurnSystem>();
            _resolver = ServiceLocator.Get<CombatResolver>();
            _selector = ServiceLocator.Get<ITargetSelector>();
            _factory  = ServiceLocator.Get<UnitFactory>();
            _input    = ServiceLocator.Get<PlayerInputHandler>();

            _bus.Subscribe<BattleEndedEvent>(e => _over = true);
            StartCoroutine(RunBattle());
        }

        // ── Battle Loop ───────────────────────────────────────────────────────

        private IEnumerator RunBattle()
        {
            yield return Setup();
            _bus.Publish(new BattleStartedEvent());
            GameStateManager.Instance?.NotifyBattleStarted();

            while (!_over)
            {
                var unit = _turns.Current;
                if (unit == null || !unit.IsAlive) { _turns.Advance(); yield return null; continue; }

                _turn++;
                _bus.Publish(new TurnStartedEvent(unit, _turn));
                Debug.Log($"[Turn {_turn}] {unit.Name} ({unit.Team}) — HP: {unit.CurrentHealth}/{unit.Stats.MaxHealth}");

                unit.TickStatusEffects(true);
                if (_over) break;

                if (unit.HasStatus(StatusEffectType.Stun))
                {
                    Debug.Log($"[Turn {_turn}] {unit.Name} is stunned — skipping turn.");
                    EndTurn(unit);
                    continue;
                }

                IBattleCommand cmd;
                if (unit.Team == TeamId.Player)
                {
                    yield return GetPlayerCommand(unit);
                    cmd = _pendingCmd;
                }
                else
                {
                    yield return new WaitForSeconds(enemyDelay);
                    cmd = GetEnemyCommand(unit);
                }

                if (cmd != null)
                {
                    yield return cmd.Execute();
                    yield return new WaitForSeconds(actionDelay);
                }

                if (_over) break;
                EndTurn(unit);
                CheckWin();
            }
        }

        private void EndTurn(CombatUnit unit)
        {
            unit.TickStatusEffects(false);
            unit.TickCooldowns();
            if (!_over) _turns.Advance();
        }

        // ── Player Input ──────────────────────────────────────────────────────

        private IEnumerator GetPlayerCommand(CombatUnit unit)
        {
            _pendingCmd = null;
            _cmdReady   = false;

            Action<PlayerAction> handler = null;
            handler = action =>
            {
                _pendingCmd = BuildCommand(action, unit);
                _cmdReady   = true;
                _input.OnActionSubmitted -= handler;
            };

            _input.OnActionSubmitted += handler;
            _input.BeginWaitingForInput(unit);

            while (!_cmdReady && !_over) yield return null;

            if (_over) { _input.OnActionSubmitted -= handler; _input.CancelInput(); }
        }

        private IBattleCommand BuildCommand(PlayerAction action, CombatUnit unit)
        {
            if (action.Type == PlayerActionType.Ability
                && action.Index >= 0 && action.Index < unit.Abilities.Count)
                return new AbilityCommand(unit, unit.Abilities[action.Index], _enemies, _selector);

            return new AttackCommand(unit, _enemies, _resolver);
        }

        // ── Enemy AI ──────────────────────────────────────────────────────────

        private IBattleCommand GetEnemyCommand(CombatUnit unit)
        {
            foreach (var ability in unit.Abilities)
                if (ability.CanUse(unit))
                    return new AbilityCommand(unit, ability, _players, _selector);

            return new AttackCommand(unit, _players, _resolver);
        }

        // ── Setup ─────────────────────────────────────────────────────────────

        private IEnumerator Setup()
        {
            _players = new List<CombatUnit>(_factory.CreateTeam(playerTeam, TeamId.Player, playerSpawns));
            _enemies = new List<CombatUnit>(_factory.CreateTeam(enemyTeam,  TeamId.Enemy,  enemySpawns));

            foreach (var u in _players) _bus.Publish(new UnitRegisteredEvent(u));
            foreach (var u in _enemies) _bus.Publish(new UnitRegisteredEvent(u));

            _turns.Initialize(_players, _enemies);
            yield break;
        }

        // ── Win Condition ─────────────────────────────────────────────────────

        private void CheckWin()
        {
            bool playerWiped = _players.TrueForAll(u => !u.IsAlive);
            bool enemyWiped  = _enemies.TrueForAll(u => !u.IsAlive);

            if (playerWiped || enemyWiped)
            {
                _over = true;
                var outcome = enemyWiped ? BattleOutcome.PlayerVictory : BattleOutcome.EnemyVictory;
                Debug.Log($"[Battle] Over — {outcome}");
                _bus.Publish(new BattleEndedEvent(outcome));

                if (enemyWiped) GameStateManager.Instance?.NotifyPlayerVictory();
                else            GameStateManager.Instance?.NotifyPlayerDefeat();
            }
        }
    }
}