using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinosBattle
{
    // Singleton that persists across scenes.
    // Owns game state, scene transitions, and time scale.
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string battleScene   = "BattleScene";

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public event Action<GameState, GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartGame()     { SetState(GameState.Loading); SceneManager.LoadScene(battleScene); }
        public void GoToMainMenu()  { Time.timeScale = 1f; SetState(GameState.Loading); SceneManager.LoadScene(mainMenuScene); }
        public void PauseGame()     { if (CurrentState == GameState.Battle)  { Time.timeScale = 0f; SetState(GameState.Paused); } }
        public void ResumeGame()    { if (CurrentState == GameState.Paused)  { Time.timeScale = 1f; SetState(GameState.Battle); } }
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void NotifyBattleStarted() => SetState(GameState.Battle);
        public void NotifyPlayerVictory() => SetState(GameState.Victory);
        public void NotifyPlayerDefeat()  => SetState(GameState.Defeat);

        public bool IsPaused   => CurrentState == GameState.Paused;
        public bool IsInBattle => CurrentState == GameState.Battle || CurrentState == GameState.Paused;

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
