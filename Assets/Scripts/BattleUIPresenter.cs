using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Game;
using DinosBattle.Infrastructure.EventBus;
using DinosBattle.Infrastructure.ServiceLocator;
using DinosBattle.Input;

namespace DinosBattle.Presenters
{
    /// <summary>
    /// BattleUIPresenter — updated to use PlayerInputHandler instead of
    /// calling methods directly on BattleCoordinator.
    ///
    /// If you are using BattleHUD (UI/Screens/BattleHUD.cs) in your scene,
    /// you can delete this file entirely — BattleHUD is the replacement.
    /// This file exists so existing scene references keep working.
    /// </summary>
    public class BattleUIPresenter : MonoBehaviour
    {
        [Header("Team HP Panels")]
        [SerializeField] private Transform         playerHpPanel;
        [SerializeField] private Transform         enemyHpPanel;
        [SerializeField] private GameObject        healthBarPrefab;

        [Header("Battle Info")]
        [SerializeField] private TextMeshProUGUI   turnLabel;
        [SerializeField] private TextMeshProUGUI   battleLog;
        [SerializeField] private int               maxLogLines = 10;

        [Header("Result")]
        [SerializeField] private GameObject        resultPanel;
        [SerializeField] private TextMeshProUGUI   resultLabel;
        [SerializeField] private Button            resultMainMenuButton;
        [SerializeField] private Button            resultQuitButton;

        [Header("Player Action Panel")]
        [SerializeField] private GameObject        actionPanel;
        [SerializeField] private Button            attackButton;
        [SerializeField] private Transform         abilityButtonPanel;
        [SerializeField] private GameObject        abilityButtonPrefab;
        [SerializeField] private TextMeshProUGUI   activeUnitLabel;

        [Header("Pause Button")]
        [SerializeField] private Button            pauseButton;

        // ── Internal ──────────────────────────────────────────────────────────
        private BattleEventBus                        _bus;
        private PlayerInputHandler                    _inputHandler;
        private readonly Dictionary<string, Slider>   _hpSliders   = new Dictionary<string, Slider>();
        private readonly Dictionary<string, TMP_Text> _hpLabels    = new Dictionary<string, TMP_Text>();
        private readonly List<string>                 _logLines    = new List<string>();
        private readonly List<GameObject>             _abilityBtns = new List<GameObject>();

        private readonly Color _playerHpColor   = new Color(0.2f,  0.8f,  0.3f);
        private readonly Color _enemyHpColor    = new Color(0.9f,  0.2f,  0.2f);
        private readonly Color _criticalHpColor = new Color(0.95f, 0.80f, 0.10f);

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (attackButton      != null) attackButton.onClick.AddListener(OnAttackClicked);
            if (pauseButton       != null) pauseButton.onClick.AddListener(OnPauseClicked);
            if (resultMainMenuButton != null) resultMainMenuButton.onClick.AddListener(() => GameStateManager.Instance?.GoToMainMenu());
            if (resultQuitButton  != null) resultQuitButton.onClick.AddListener(() => GameStateManager.Instance?.QuitGame());

            SetActionPanelVisible(false);
            if (resultPanel != null) resultPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (BattleServices.TryGet<BattleEventBus>(out _bus))
            {
                _bus.Subscribe<BattleStartedEvent>(OnBattleStarted);
                _bus.Subscribe<TurnStartedEvent>(OnTurnStarted);
                _bus.Subscribe<AttackExecutedEvent>(OnAttackExecuted);
                _bus.Subscribe<HealthChangedEvent>(OnHealthChanged);
                _bus.Subscribe<UnitDefeatedEvent>(OnUnitDefeated);
                // _bus.Subscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied);
                _bus.Subscribe<BattleEndedEvent>(OnBattleEnded);
                _bus.Subscribe<UnitRegisteredEvent>(OnUnitRegistered);
            }

            // Subscribe to PlayerInputHandler events — no coordinator reference needed
            if (BattleServices.TryGet<PlayerInputHandler>(out _inputHandler))
            {
                _inputHandler.OnInputWindowOpened += OnInputWindowOpened;
                _inputHandler.OnInputWindowClosed += OnInputWindowClosed;
            }
        }

        private void OnDisable()
        {
            if (_bus != null)
            {
                _bus.Unsubscribe<BattleStartedEvent>(OnBattleStarted);
                _bus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
                _bus.Unsubscribe<AttackExecutedEvent>(OnAttackExecuted);
                _bus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
                _bus.Unsubscribe<UnitDefeatedEvent>(OnUnitDefeated);
                // _bus.Unsubscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied);
                _bus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
            }
                _bus.Unsubscribe<UnitRegisteredEvent>(OnUnitRegistered);

            if (_inputHandler != null)
            {
                _inputHandler.OnInputWindowOpened -= OnInputWindowOpened;
                _inputHandler.OnInputWindowClosed -= OnInputWindowClosed;
            }
        }

        // ── Unit registration — driven by UnitRegisteredEvent ────────────────

        private void OnUnitRegistered(UnitRegisteredEvent e) => SpawnHpBar(e.Unit);

        private void SpawnHpBar(CombatUnit unit)
        {
            Transform panel = unit.Team == TeamId.Player ? playerHpPanel : enemyHpPanel;
            if (panel == null || healthBarPrefab == null)
            {
                Debug.LogWarning($"[BattleUIPresenter] Cannot spawn HP bar for {unit.Name}: " +
                    (panel == null ? "panel is null" : "healthBarPrefab is null"));
                return;
            }

            var go    = Instantiate(healthBarPrefab, panel);
            go.name   = "HP_" + unit.Name;

            var slider = go.GetComponentInChildren<Slider>();
            var label  = go.GetComponentInChildren<TMP_Text>();
            Color col  = unit.Team == TeamId.Player ? _playerHpColor : _enemyHpColor;

            if (slider != null)
            {
                slider.maxValue         = unit.BaseStats.MaxHealth;
                slider.value            = unit.CurrentHealth;
                SetSliderColor(slider, col);
                _hpSliders[unit.UnitId] = slider;
            }
            if (label != null)
            {
                label.text             = FormatHp(unit);
                _hpLabels[unit.UnitId] = label;
            }

            Debug.Log($"[BattleUIPresenter] Spawned HP bar for {unit.Name} on {panel.name}");
        }

        // ── EventBus handlers ─────────────────────────────────────────────────

        private void OnBattleStarted(BattleStartedEvent e)
        {
            if (resultPanel != null) resultPanel.SetActive(false);
            AppendLog("Battle started!");
        }

        private void OnTurnStarted(TurnStartedEvent e)
        {
            string side = e.Unit.Team == TeamId.Player ? "(You)" : "(Enemy)";
            if (turnLabel != null)
                turnLabel.text = $"{e.Unit.Name} {side}  |  Turn {e.TurnNumber}";
        }

        private void OnAttackExecuted(AttackExecutedEvent e)
        {
            var r    = e.Result;
            string c = r.IsCritical ? " [CRIT!]" : "";
            string m = r.IsMiss     ? " [MISS]"  : "";
            AppendLog($"{r.Attacker?.Name} -> {r.Defender?.Name}: {r.FinalDamage} dmg{c}{m}");
        }

        private void OnHealthChanged(HealthChangedEvent e) => RefreshBar(e.Unit);

        private void OnUnitDefeated(UnitDefeatedEvent e)
        {
            RefreshBar(e.Unit);
            AppendLog($"{e.Unit.Name} defeated!");
        }

        // private void OnStatusEffectApplied(StatusEffectAppliedEvent e) =>
        //     AppendLog($"{e.Unit.Name} -> [{e.Effect.EffectName}]");

        private void OnBattleEnded(BattleEndedEvent e)
        {
            SetActionPanelVisible(false);
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultLabel != null)
            {
                bool won = e.Outcome == BattleOutcome.PlayerVictory;
                resultLabel.text = won
                    ? $"VICTORY!\n{e.TotalTurns} turns"
                    : $"DEFEAT\n{e.TotalTurns} turns";
            }
        }

        // ── Input window (from PlayerInputHandler) ────────────────────────────

        private void OnInputWindowOpened(CombatUnit unit)
        {
            if (activeUnitLabel != null)
                activeUnitLabel.text = $"Choose action: {unit.Name}";

            BuildAbilityButtons(unit);
            SetActionPanelVisible(true);
            AppendLog($"Your turn! Choose an action for {unit.Name}.");
        }

        private void OnInputWindowClosed() => SetActionPanelVisible(false);

        // ── Button callbacks ──────────────────────────────────────────────────

        private void OnAttackClicked()
        {
            // Route through PlayerInputHandler — no direct coordinator call
            _inputHandler?.SubmitAttack();
        }

        private void OnPauseClicked() => GameStateManager.Instance?.PauseGame();

        private void BuildAbilityButtons(CombatUnit unit)
        {
            foreach (var b in _abilityBtns)
                if (b != null) Destroy(b);
            _abilityBtns.Clear();

            if (abilityButtonPanel == null || abilityButtonPrefab == null) return;

            for (int i = 0; i < unit.Abilities.Count; i++)
            {
                var ability = unit.Abilities[i];
                var go      = Instantiate(abilityButtonPrefab, abilityButtonPanel);
                go.name     = "AbilityBtn_" + ability.AbilityName;
                _abilityBtns.Add(go);

                bool onCD  = unit.IsOnCooldown(ability.AbilityName);
                var label  = go.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = onCD ? $"{ability.AbilityName}\n(CD)" : ability.AbilityName;

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = !onCD;
                    int idx = i;
                    // Route through PlayerInputHandler — no direct coordinator call
                    btn.onClick.AddListener(() => _inputHandler?.SubmitAbility(idx));
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetActionPanelVisible(bool visible)
        {
            if (actionPanel != null) actionPanel.SetActive(visible);
        }

        private void RefreshBar(CombatUnit unit)
        {
            if (_hpSliders.TryGetValue(unit.UnitId, out var slider))
            {
                slider.value = unit.CurrentHealth;
                if (unit.HealthPercent < 0.3f)
                    SetSliderColor(slider, _criticalHpColor);
            }
            if (_hpLabels.TryGetValue(unit.UnitId, out var label))
                label.text = FormatHp(unit);
        }

        private static string FormatHp(CombatUnit unit) =>
            $"{unit.Name}: {unit.CurrentHealth}/{unit.BaseStats.MaxHealth}";

        private void AppendLog(string line)
        {
            _logLines.Add(line);
            if (_logLines.Count > maxLogLines) _logLines.RemoveAt(0);
            if (battleLog != null) battleLog.text = string.Join("\n", _logLines);
        }

        private static void SetSliderColor(Slider slider, Color color)
        {
            if (slider.fillRect == null) return;
            var img = slider.fillRect.GetComponent<Image>();
            if (img != null) img.color = color;
        }
    }
}