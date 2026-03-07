using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinosBattle.Game
{
    // ═══════════════════════════════════════════════════════════════════════
    //  GAME STATES
    // ═══════════════════════════════════════════════════════════════════════

    public enum GameState
    {
        MainMenu,
        Loading,
        Battle,
        Paused,
        Victory,
        Defeat
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GAME STATE MANAGER  (Singleton — persists across scenes)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Central game lifecycle controller.
    /// Owns scene transitions, pause state, and time scale.
    /// Raises OnStateChanged so any screen can react without polling.
    ///
    /// Singleton — survives scene loads. One instance for the entire session.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static GameStateManager Instance { get; private set; }

        // ── Scene names (must match Build Settings) ───────────────────────────
        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string battleSceneName   = "BattleScene";

        // ── State ─────────────────────────────────────────────────────────────
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<GameState, GameState> OnStateChanged;  // (previous, next)

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  PUBLIC API
        // ═════════════════════════════════════════════════════════════════════

        public void StartGame()
        {
            SetState(GameState.Loading);
            SceneManager.LoadScene(battleSceneName);
            // BattleScene's Awake/Start will call SetState(GameState.Battle)
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.Loading);
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Battle) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Battle);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void NotifyBattleStarted()  => SetState(GameState.Battle);
        public void NotifyPlayerVictory()  => SetState(GameState.Victory);
        public void NotifyPlayerDefeat()   => SetState(GameState.Defeat);

        public bool IsPaused     => CurrentState == GameState.Paused;
        public bool IsInBattle   => CurrentState == GameState.Battle || CurrentState == GameState.Paused;

        // ── Private ───────────────────────────────────────────────────────────

        private void SetState(GameState next)
        {
            if (CurrentState == next) return;
            var prev = CurrentState;
            CurrentState = next;
            Debug.Log($"[GameState] {prev} → {next}");
            OnStateChanged?.Invoke(prev, next);
        }
    }
}