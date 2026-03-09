using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DinosBattle.Input;

namespace DinosBattle.UI
{
    // All battle UI in one place: HP bars, turn label, battle log,
    // player action panel, pause button, and result overlay.
    public class BattleHUD : MonoBehaviour
    {
        [Header("HP Panels")]
        [SerializeField] private Transform playerHpPanel;
        [SerializeField] private Transform enemyHpPanel;
        [SerializeField] private GameObject healthBarPrefab;

        [Header("Battle Info")]
        [SerializeField] private TextMeshProUGUI turnLabel;
        [SerializeField] private TextMeshProUGUI battleLog;
        [SerializeField] private int maxLogLines = 10;

        [Header("Action Panel")]
        [SerializeField] private CanvasGroup actionPanel;
        [SerializeField] private Button attackButton;
        [SerializeField] private Transform abilityContainer;
        [SerializeField] private GameObject abilityButtonPrefab;
        [SerializeField] private TextMeshProUGUI activeUnitLabel;

        [Header("Pause")]
        [SerializeField] private Button pauseButton;

        [Header("Result Overlay")]
        [SerializeField] private GameObject resultOverlay;
        [SerializeField] private TextMeshProUGUI resultTitle;
        [SerializeField] private TextMeshProUGUI resultSub;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        private EventBus _eventBus;
        private PlayerInputHandler _playerInput;
        private readonly Dictionary<string, Slider> _sliders = new Dictionary<string, Slider>();
        private readonly Dictionary<string, TMP_Text> _hpLabels = new Dictionary<string, TMP_Text>();
        private readonly List<string> _log = new List<string>();
        private readonly List<GameObject> _abilityBtns = new List<GameObject>();

        private readonly Color _playerColor = new Color(0.25f, 0.85f, 0.35f);
        private readonly Color _enemyColor = new Color(0.90f, 0.25f, 0.20f);
        private readonly Color _criticalColor = new Color(0.95f, 0.80f, 0.10f);

        private void Awake()
        {
            attackButton?.onClick.AddListener(() => _playerInput?.SubmitAttack());
            pauseButton?.onClick.AddListener(() => GameStateManager.Instance?.PauseGame());
            mainMenuButton?.onClick.AddListener(() => GameStateManager.Instance?.GoToMainMenu());
            quitButton?.onClick.AddListener(() => GameStateManager.Instance?.QuitGame());

            SetActionPanel(false);
            resultOverlay?.SetActive(false);
        }

        private void OnEnable()
        {
            if (!ServiceLocator.TryGet<EventBus>(out _eventBus)) return;

            _eventBus.Subscribe<BattleStartedEvent>(OnBattleStarted);
            _eventBus.Subscribe<UnitRegisteredEvent>(OnUnitRegistered);
            _eventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            _eventBus.Subscribe<AttackExecutedEvent>(OnAttackExecuted);
            _eventBus.Subscribe<HealthChangedEvent>(OnHealthChanged);
            _eventBus.Subscribe<UnitDefeatedEvent>(OnUnitDefeated);
            _eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        }

        private void OnDisable()
        {
            if (_eventBus == null) return;

            _eventBus.Unsubscribe<BattleStartedEvent>(OnBattleStarted);
            _eventBus.Unsubscribe<UnitRegisteredEvent>(OnUnitRegistered);
            _eventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            _eventBus.Unsubscribe<AttackExecutedEvent>(OnAttackExecuted);
            _eventBus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
            _eventBus.Unsubscribe<UnitDefeatedEvent>(OnUnitDefeated);
            _eventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);

            if (_playerInput != null)
            {
                _playerInput.OnWindowOpened -= OnInputWindowOpened;
                _playerInput.OnWindowClosed -= OnInputWindowClosed;
            }
        }

        // BattleStarted fires after all Awake/Start — safe to resolve PlayerInputHandler here.
        private void OnBattleStarted(BattleStartedEvent battleStartedEvent)
        {
            if (ServiceLocator.TryGet<PlayerInputHandler>(out _playerInput))
            {
                _playerInput.OnWindowOpened += OnInputWindowOpened;
                _playerInput.OnWindowClosed += OnInputWindowClosed;
            }
            resultOverlay?.SetActive(false);
            AddLog("Battle started!");
        }

        private void OnUnitRegistered(UnitRegisteredEvent unitRegisteredEvent)
        {
            var combatUnit = unitRegisteredEvent.Unit;
            var panel = combatUnit.Team == TeamId.Player ? playerHpPanel : enemyHpPanel;
            if (panel == null || healthBarPrefab == null) return;

            var go = Instantiate(healthBarPrefab, panel);
            go.name = "HP_" + combatUnit.Name;
            var slider = go.GetComponentInChildren<Slider>();
            var label = go.GetComponentInChildren<TMP_Text>();
            var color = combatUnit.Team == TeamId.Player ? _playerColor : _enemyColor;

            if (slider != null)
            {
                slider.maxValue = combatUnit.Stats.MaxHealth;
                slider.value = combatUnit.CurrentHealth;
                SetFillColor(slider, color);
                _sliders[combatUnit.Name] = slider;
            }
            if (label != null)
            {
                label.text = HpText(combatUnit);
                _hpLabels[combatUnit.Name] = label;
            }
        }

        private void OnTurnStarted(TurnStartedEvent turnStartedEvent)
        {
            if (turnLabel == null) return;
            string side = turnStartedEvent.Unit.Team == TeamId.Player ? "Your turn" : "Enemy turn";
            turnLabel.text = $"{turnStartedEvent.Unit.Name}  |  {side}  |  Turn {turnStartedEvent.Turn}";
        }

        private void OnAttackExecuted(AttackExecutedEvent attackExecutedEvent)
        {
            var attackResult = attackExecutedEvent.Result;
            AddLog($"[Attack] {attackResult.Attacker?.Name} attacked {attackResult.Defender?.Name}, dealing {attackResult.Damage} damage. " +
                               $"{(attackResult.IsMiss ? "The attack missed!" : attackResult.IsCrit ? "Critical hit!" : "")}");
        }

        private void OnHealthChanged(HealthChangedEvent healthChangedEvent) => RefreshBar(healthChangedEvent.Unit);

        private void OnUnitDefeated(UnitDefeatedEvent unitDefeatedEvent)
        {
            RefreshBar(unitDefeatedEvent.Unit);
            AddLog($"{unitDefeatedEvent.Unit.Name} defeated!");
        }

        private void OnBattleEnded(BattleEndedEvent battleEndedEvent)
        {
            SetActionPanel(false);
            if (resultOverlay == null) return;
            resultOverlay.SetActive(true);
            bool won = battleEndedEvent.Outcome == BattleOutcome.PlayerVictory;
            if (resultTitle != null) resultTitle.text = won ? "VICTORY!" : "DEFEAT";
            if (resultSub != null) resultSub.text = won ? "All enemies defeated!" : "Your team was wiped out.";
        }

        private void OnInputWindowOpened(CombatUnit unit)
        {
            if (activeUnitLabel != null) activeUnitLabel.text = $"Choose action: {unit.Name}";
            BuildAbilityButtons(unit);
            SetActionPanel(true);
        }

        private void OnInputWindowClosed() => SetActionPanel(false);

        private void BuildAbilityButtons(CombatUnit unit)
        {
            foreach (var abilityButton in _abilityBtns) if (abilityButton) Destroy(abilityButton);
            _abilityBtns.Clear();
            if (abilityContainer == null || abilityButtonPrefab == null) return;

            for (int i = 0; i < unit.Abilities.Count; i++)
            {
                var ability = unit.Abilities[i];
                bool onCD = unit.IsOnCooldown(ability.Name);
                var go = Instantiate(abilityButtonPrefab, abilityContainer);
                _abilityBtns.Add(go);

                var label = go.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = onCD ? $"{ability.Name}\n(Cooling down)" : ability.Name;

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = !onCD;
                    int idx = i;
                    btn.onClick.AddListener(() => _playerInput?.SubmitAbility(idx));
                }
            }
        }

        private void RefreshBar(CombatUnit unit)
        {
            if (_sliders.TryGetValue(unit.Name, out var s))
            {
                s.value = unit.CurrentHealth;
                if (unit.HealthPercent < 0.3f) SetFillColor(s, _criticalColor);
            }
            if (_hpLabels.TryGetValue(unit.Name, out var l)) l.text = HpText(unit);
        }

        private void SetActionPanel(bool isInteractable) { actionPanel.interactable = isInteractable; }

        private void AddLog(string line)
        {
            Debug.Log(line);
            _log.Add(line);
            if (_log.Count > maxLogLines) _log.RemoveAt(0);
            if (battleLog != null) battleLog.text = string.Join("\n", _log);
        }

        private static string HpText(CombatUnit u) => $"{u.Name}  {u.CurrentHealth}/{u.Stats.MaxHealth}";

        private static void SetFillColor(Slider slider, Color color)
        {
            var img = slider.fillRect?.GetComponent<Image>();
            if (img != null) img.color = color;
        }
    }
}
