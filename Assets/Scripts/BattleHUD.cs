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
        [SerializeField] private Transform  playerHpPanel;
        [SerializeField] private Transform  enemyHpPanel;
        [SerializeField] private GameObject healthBarPrefab;

        [Header("Battle Info")]
        [SerializeField] private TextMeshProUGUI turnLabel;
        [SerializeField] private TextMeshProUGUI battleLog;
        [SerializeField] private int             maxLogLines = 10;

        [Header("Action Panel")]
        [SerializeField] private GameObject      actionPanel;
        [SerializeField] private Button          attackButton;
        [SerializeField] private Transform       abilityContainer;
        [SerializeField] private GameObject      abilityButtonPrefab;
        [SerializeField] private TextMeshProUGUI activeUnitLabel;

        [Header("Pause")]
        [SerializeField] private Button pauseButton;

        [Header("Result Overlay")]
        [SerializeField] private GameObject      resultOverlay;
        [SerializeField] private TextMeshProUGUI resultTitle;
        [SerializeField] private TextMeshProUGUI resultSub;
        [SerializeField] private Button          mainMenuButton;
        [SerializeField] private Button          quitButton;

        private EventBus                              _bus;
        private PlayerInputHandler                    _input;
        private readonly Dictionary<string, Slider>   _sliders    = new Dictionary<string, Slider>();
        private readonly Dictionary<string, TMP_Text> _hpLabels   = new Dictionary<string, TMP_Text>();
        private readonly List<string>                 _log        = new List<string>();
        private readonly List<GameObject>             _abilityBtns = new List<GameObject>();

        private readonly Color _playerColor   = new Color(0.25f, 0.85f, 0.35f);
        private readonly Color _enemyColor    = new Color(0.90f, 0.25f, 0.20f);
        private readonly Color _criticalColor = new Color(0.95f, 0.80f, 0.10f);

        private void Awake()
        {
            attackButton  ?.onClick.AddListener(() => _input?.SubmitAttack());
            pauseButton   ?.onClick.AddListener(() => GameStateManager.Instance?.PauseGame());
            mainMenuButton?.onClick.AddListener(() => GameStateManager.Instance?.GoToMainMenu());
            quitButton    ?.onClick.AddListener(() => GameStateManager.Instance?.QuitGame());

            SetActionPanel(false);
            resultOverlay?.SetActive(false);
        }

        private void OnEnable()
        {
            if (!ServiceLocator.TryGet<EventBus>(out _bus)) return;

            _bus.Subscribe<BattleStartedEvent>(OnBattleStarted);
            _bus.Subscribe<UnitRegisteredEvent>(OnUnitRegistered);
            _bus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            _bus.Subscribe<AttackExecutedEvent>(OnAttackExecuted);
            _bus.Subscribe<HealthChangedEvent>(OnHealthChanged);
            _bus.Subscribe<UnitDefeatedEvent>(OnUnitDefeated);
            _bus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        }

        private void OnDisable()
        {
            if (_bus == null) return;

            _bus.Unsubscribe<BattleStartedEvent>(OnBattleStarted);
            _bus.Unsubscribe<UnitRegisteredEvent>(OnUnitRegistered);
            _bus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            _bus.Unsubscribe<AttackExecutedEvent>(OnAttackExecuted);
            _bus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
            _bus.Unsubscribe<UnitDefeatedEvent>(OnUnitDefeated);
            _bus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);

            if (_input != null)
            {
                _input.OnWindowOpened -= OnInputWindowOpened;
                _input.OnWindowClosed -= OnInputWindowClosed;
            }
        }

        // BattleStarted fires after all Awake/Start — safe to resolve PlayerInputHandler here.
        private void OnBattleStarted(BattleStartedEvent e)
        {
            if (ServiceLocator.TryGet<PlayerInputHandler>(out _input))
            {
                _input.OnWindowOpened += OnInputWindowOpened;
                _input.OnWindowClosed += OnInputWindowClosed;
            }
            resultOverlay?.SetActive(false);
            AddLog("Battle started!");
        }

        private void OnUnitRegistered(UnitRegisteredEvent e)
        {
            var unit  = e.Unit;
            var panel = unit.Team == TeamId.Player ? playerHpPanel : enemyHpPanel;
            if (panel == null || healthBarPrefab == null) return;

            var go     = Instantiate(healthBarPrefab, panel);
            go.name    = "HP_" + unit.Name;
            var slider = go.GetComponentInChildren<Slider>();
            var label  = go.GetComponentInChildren<TMP_Text>();
            var color  = unit.Team == TeamId.Player ? _playerColor : _enemyColor;

            if (slider != null)
            {
                slider.maxValue           = unit.Stats.MaxHealth;
                slider.value              = unit.CurrentHealth;
                SetFillColor(slider, color);
                _sliders[unit.Name]       = slider;
            }
            if (label != null)
            {
                label.text                = HpText(unit);
                _hpLabels[unit.Name]      = label;
            }
        }

        private void OnTurnStarted(TurnStartedEvent e)
        {
            if (turnLabel == null) return;
            string side = e.Unit.Team == TeamId.Player ? "Your turn" : "Enemy turn";
            turnLabel.text = $"{e.Unit.Name}  |  {side}  |  Turn {e.Turn}";
        }

        private void OnAttackExecuted(AttackExecutedEvent e)
        {
            var r = e.Result;
            AddLog($"{r.Attacker?.Name} → {r.Defender?.Name}: {r.Damage} dmg" +
                   (r.IsMiss ? " MISS" : r.IsCrit ? " CRIT!" : ""));
        }

        private void OnHealthChanged(HealthChangedEvent e) => RefreshBar(e.Unit);

        private void OnUnitDefeated(UnitDefeatedEvent e)
        {
            RefreshBar(e.Unit);
            AddLog($"{e.Unit.Name} defeated!");
        }

        private void OnBattleEnded(BattleEndedEvent e)
        {
            SetActionPanel(false);
            if (resultOverlay == null) return;
            resultOverlay.SetActive(true);
            bool won = e.Outcome == BattleOutcome.PlayerVictory;
            if (resultTitle != null) resultTitle.text = won ? "VICTORY!" : "DEFEAT";
            if (resultSub   != null) resultSub.text   = won ? "All enemies defeated!" : "Your team was wiped out.";
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
            foreach (var b in _abilityBtns) if (b) Destroy(b);
            _abilityBtns.Clear();
            if (abilityContainer == null || abilityButtonPrefab == null) return;

            for (int i = 0; i < unit.Abilities.Count; i++)
            {
                var ability = unit.Abilities[i];
                bool onCD   = unit.IsOnCooldown(ability.Name);
                var go      = Instantiate(abilityButtonPrefab, abilityContainer);
                _abilityBtns.Add(go);

                var label = go.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = onCD ? $"{ability.Name}\n(CD)" : ability.Name;

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = !onCD;
                    int idx = i;
                    btn.onClick.AddListener(() => _input?.SubmitAbility(idx));
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

        private void SetActionPanel(bool on) { if (actionPanel) actionPanel.SetActive(on); }

        private void AddLog(string line)
        {
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
