using UnityEngine;
using UnityEngine.UI;

namespace DinosBattle.UI.Screens
{
    /// <summary>
    /// Pause menu — shown over the battle when the player presses Escape or the pause button.
    /// Uses unscaledDeltaTime so animations still run while Time.timeScale == 0.
    /// </summary>
    public class PauseMenuScreen : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject    pausePanel;
        [SerializeField] private CanvasGroup   panelGroup;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        private bool _visible;

        private void Awake()
        {
            if (resumeButton   != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            if (quitButton     != null) quitButton.onClick.AddListener(OnQuitClicked);

            SetVisible(false, instant: true);

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        // private void Update()
        // {
        //     // Escape key toggles pause from anywhere in battle
        //     if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) &&
        //         GameStateManager.Instance != null &&
        //         GameStateManager.Instance.IsInBattle)
        //     {
        //         if (GameStateManager.Instance.IsPaused)
        //             OnResumeClicked();
        //         else
        //             OnPauseRequested();
        //     }
        // }

        // ── Button handlers ───────────────────────────────────────────────────

        private void OnPauseRequested()
        {
            GameStateManager.Instance?.PauseGame();
        }

        private void OnResumeClicked()
        {
            GameStateManager.Instance?.ResumeGame();
        }

        private void OnMainMenuClicked()
        {
            GameStateManager.Instance?.GoToMainMenu();
        }

        private void OnQuitClicked()
        {
            GameStateManager.Instance?.QuitGame();
        }

        // ── React to GameStateManager ─────────────────────────────────────────

        private void OnGameStateChanged(GameState previous, GameState next)
        {
            if (next == GameState.Paused)  SetVisible(true);
            if (next == GameState.Battle)  SetVisible(false);
            if (next == GameState.Victory || next == GameState.Defeat) SetVisible(false);
        }

        // ── Show / hide with fade ─────────────────────────────────────────────

        private void SetVisible(bool visible, bool instant = false)
        {
            _visible = visible;
            if (pausePanel != null) pausePanel.SetActive(visible);

            if (panelGroup != null)
            {
                if (instant)
                {
                    panelGroup.alpha = visible ? 1f : 0f;
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeTo(visible ? 1f : 0f));
                }
            }
        }

        private System.Collections.IEnumerator FadeTo(float target)
        {
            float start   = panelGroup.alpha;
            float elapsed = 0f;
            float duration = 0.25f;

            while (elapsed < duration)
            {
                panelGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            panelGroup.alpha = target;
        }
    }
}