using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Core.Interfaces;
using DinosBattle.Data;
using DinosBattle.Commands;
using DinosBattle.Game;
using DinosBattle.Infrastructure.EventBus;
using DinosBattle.Infrastructure.ServiceLocator;
using DinosBattle.Input;
using DinosBattle.Systems;
using DinosBattle.Systems.Combat;
using DinosBattle.Systems.Spawn;

namespace DinosBattle
{
    [DefaultExecutionOrder(-100)]
    public class BattleCoordinator : MonoBehaviour
    {
        [Header("Teams")]
        [SerializeField] private DinosaurData[] playerTeamData;
        [SerializeField] private DinosaurData[] enemyTeamData;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] playerSpawnPoints;
        [SerializeField] private Transform[] enemySpawnPoints;

        [Header("Pacing")]
        [SerializeField] private float enemyTurnDelay  = 1.2f;
        [SerializeField] private float postActionDelay = 0.5f;

        private BattleEventBus     _bus;
        private ITurnSystem        _turns;
        private ITurnOrderStrategy _turnStrategy;
        private IAnimationHandler  _anim;
        private ITargetSelector    _selector;
        private CombatUnitFactory  _factory;
        private CombatResolver     _resolver;
        private PlayerInputHandler _input;

        private BattleTeam _playerTeam;
        private BattleTeam _enemyTeam;

        private int  _turnCount;
        private bool _battleOver;

        private IBattleCommand _playerCommand;
        private bool           _playerReady;

        private void Start()
        {
            _bus          = BattleServices.Get<BattleEventBus>();
            _turns        = BattleServices.Get<ITurnSystem>();
            _turnStrategy = BattleServices.Get<ITurnOrderStrategy>();
            _anim         = BattleServices.Get<IAnimationHandler>();
            _selector     = BattleServices.Get<ITargetSelector>();
            _factory      = BattleServices.Get<CombatUnitFactory>();
            _resolver     = BattleServices.Get<CombatResolver>();
            _input        = BattleServices.Get<PlayerInputHandler>();

            _bus.Subscribe<BattleEndedEvent>(e => _battleOver = true);
            StartCoroutine(RunBattle());
        }

        private IEnumerator RunBattle()
        {
            yield return Setup();
            _bus.Publish(new BattleStartedEvent());
            GameStateManager.Instance?.NotifyBattleStarted();

            while (!_battleOver)
            {
                var unit = _turns.Current;
                if (unit == null || !unit.IsAlive) { _turns.Advance(); yield return null; continue; }

                _turnCount++;
                _bus.Publish(new TurnStartedEvent(unit, _turnCount));

                // Tick status effects at turn start
                unit.TickStatusEffects(true);
                if (_battleOver) break;

                // Skip if stunned
                if (unit.HasStatus(StatusEffectType.Stun))
                {
                    Debug.Log($"[Battle] {unit.Name} is stunned.");
                    unit.TickStatusEffects(false);
                    unit.TickCooldowns();
                    _turns.Advance();
                    continue;
                }

                IBattleCommand cmd;
                if (unit.Team == TeamId.Player)
                {
                    yield return WaitForPlayerInput(unit);
                    cmd = _playerCommand;
                }
                else
                {
                    yield return new WaitForSeconds(enemyTurnDelay);
                    cmd = GetEnemyCommand(unit);
                }

                if (cmd != null)
                {
                    yield return cmd.Execute();
                    yield return new WaitForSeconds(postActionDelay);
                }

                if (_battleOver) break;

                // Tick status effects at turn end
                unit.TickStatusEffects(false);
                unit.TickCooldowns();
                CheckWinCondition();
                if (!_battleOver) _turns.Advance();
            }
        }

        private IEnumerator WaitForPlayerInput(CombatUnit unit)
        {
            _playerCommand = null;
            _playerReady   = false;

            Action<PlayerAction> onAction = null;
            onAction = action =>
            {
                _playerCommand = BuildPlayerCommand(action, unit);
                _playerReady   = true;
                _input.OnActionSubmitted -= onAction;
            };
            _input.OnActionSubmitted += onAction;
            _input.BeginWaitingForInput(unit);

            while (!_playerReady && !_battleOver)
                yield return null;

            if (_battleOver)
            {
                _input.OnActionSubmitted -= onAction;
                _input.CancelInput();
            }
        }

        private IBattleCommand BuildPlayerCommand(PlayerAction action, CombatUnit unit)
        {
            if (action.Type == PlayerActionType.UseAbility
                && action.AbilityIndex >= 0
                && action.AbilityIndex < unit.Abilities.Count)
            {
                return new UseAbilityCommand(unit, unit.Abilities[action.AbilityIndex],
                    _enemyTeam.Members, _selector, _anim);
            }
            return new BasicAttackCommand(unit, _enemyTeam.Members, _resolver, _anim);
        }

        private IBattleCommand GetEnemyCommand(CombatUnit unit)
        {
            foreach (var ability in unit.Abilities)
                if (ability.CanUse(unit))
                    return new UseAbilityCommand(unit, ability, _playerTeam.Members, _selector, _anim);

            return new BasicAttackCommand(unit, _playerTeam.Members, _resolver, _anim);
        }

        private IEnumerator Setup()
        {
            _playerTeam = new BattleTeam(TeamId.Player, _bus);
            _enemyTeam  = new BattleTeam(TeamId.Enemy,  _bus);

            var pu = _factory.CreateTeam(playerTeamData, TeamId.Player, playerSpawnPoints);
            var eu = _factory.CreateTeam(enemyTeamData,  TeamId.Enemy,  enemySpawnPoints);

            foreach (var u in pu) _playerTeam.AddMember(u);
            foreach (var u in eu) _enemyTeam.AddMember(u);

            foreach (var u in pu) _bus.Publish(new UnitRegisteredEvent(u));
            foreach (var u in eu) _bus.Publish(new UnitRegisteredEvent(u));

            var all = new List<CombatUnit>(pu);
            all.AddRange(eu);
            _turns.Initialize(all, _turnStrategy);
            yield break;
        }

        private void CheckWinCondition()
        {
            if (_playerTeam.IsWiped)
            {
                _battleOver = true;
                _bus.Publish(new BattleEndedEvent(BattleOutcome.EnemyVictory, _turnCount));
                GameStateManager.Instance?.NotifyPlayerDefeat();
            }
            else if (_enemyTeam.IsWiped)
            {
                _battleOver = true;
                _bus.Publish(new BattleEndedEvent(BattleOutcome.PlayerVictory, _turnCount));
                GameStateManager.Instance?.NotifyPlayerVictory();
            }
        }
    }
}
